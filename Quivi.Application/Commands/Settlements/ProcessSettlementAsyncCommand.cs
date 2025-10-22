using Quivi.Application.Commands.People;
using Quivi.Application.Extensions;
using Quivi.Application.Queries.Journals;
using Quivi.Application.Queries.MerchantAcquirerConfigurations;
using Quivi.Application.Queries.MerchantServices;
using Quivi.Application.Queries.People;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.Settlements
{
    public class ProcessSettlementAsyncCommand : ICommand<Task>
    {
        public required DateOnly Date { get; init; }
    }

    internal class ProcessSettlementAsyncCommandHandler : ICommandHandler<ProcessSettlementAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IJournalsRepository journalsRepository;

        public ProcessSettlementAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                    ICommandProcessor commandProcessor,
                                                    IDateTimeProvider dateTimeProvider,
                                                    IJournalsRepository journalsRepository)
        {
            this.queryProcessor = queryProcessor;
            this.dateTimeProvider = dateTimeProvider;
            this.commandProcessor = commandProcessor;
            this.journalsRepository = journalsRepository;
        }

        public async Task Handle(ProcessSettlementAsyncCommand command)
        {
            var fromDate = command.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).Add(QuiviConstants.SettlementOffset);
            var toDate = fromDate.AddDays(1);

            var now = dateTimeProvider.GetUtcNow();

            if (now < toDate)
                return;

            await commandProcessor.Execute(new UpsertSettlementAsyncCommand
            {
                Date = command.Date,
                UpdateAction = async settlement =>
                {
                    try
                    {
                        var journalsQuery = await queryProcessor.Execute(new GetJournalsAsyncQuery
                        {
                            States = [JournalState.Completed],
                            Types = [JournalType.Capture, JournalType.Refund],
                            FromDate = fromDate,
                            ToDate = toDate,

                            IncludeJournalDetails = true,
                            IncludeMerchantFees = true,
                            IncludeSubMerchantFees = true,

                            PageIndex = 0,
                            PageSize = null,
                        });

                        var merchantsConfigurationsQuery = await queryProcessor.Execute(new GetMerchantAcquirerConfigurationsAsyncQuery
                        {
                            MerchantIds = journalsQuery.Select(j =>
                            {
                                var channelPosting = j.Postings!.First(p => p.Person!.PersonType == PersonType.Channel);
                                return channelPosting!.Person!.MerchantId ?? 0;
                            }).Where(id => id != 0).Distinct(),

                            IsDeleted = false,

                            PageIndex = 0,
                            PageSize = null,
                        });
                        var merchantsConfigurations = merchantsConfigurationsQuery.GroupBy(a => a.MerchantId).ToDictionary(a => a.Key, a => a.ToDictionary(b => b.ChargeMethod, b => b) as IReadOnlyDictionary<ChargeMethod, MerchantAcquirerConfiguration>);

                        ProcessSettlementDetails(settlement, journalsQuery, merchantsConfigurations, now);
                        await ProcessSettlementServiceDetails(settlement, now);

                        settlement.State = SettlementState.Finished;
                    }
                    catch
                    {
                        settlement.State = SettlementState.Failed;
                    }
                },
            });
        }

        public void ProcessSettlementDetails(IUpdatableSettlement settlement, IEnumerable<Journal> journals, IReadOnlyDictionary<int, IReadOnlyDictionary<ChargeMethod, MerchantAcquirerConfiguration>> merchantConfigurationsDictionary, DateTime now)
        {
            foreach (var journal in journals)
            {
                var channelPosting = journal.Postings!.First(p => p.Person!.PersonType == PersonType.Channel);
                var consumerPosting = journal.Postings!.First(p => p.Person!.PersonType == PersonType.Consumer);
                var channel = channelPosting.Person!;

                if (channel.MerchantId.HasValue &&
                    merchantConfigurationsDictionary.TryGetValue(channel.MerchantId.Value, out var config) &&
                    config.TryGetValue(journal.ChargeMethod!.Value, out var acquirerConfigurarion) &&
                    acquirerConfigurarion.ExternallySettled)
                {
                    continue;
                }

                var subMerchant = channel.Merchant!;

                var fees = subMerchant.TransactionFees?.ToDictionary(r => r.ChargeMethod, r => r) ?? new Dictionary<ChargeMethod, MerchantFee>();
                decimal fee = subMerchant.TransactionFee;
                FeeUnit feeUnit = subMerchant.TransactionFeeUnit;

                if (fees != null && fees.TryGetValue(journal.ChargeMethod!.Value, out var merchantFee))
                {
                    fee = merchantFee.Fee;
                    feeUnit = merchantFee.FeeUnit;
                }

                var feeAmount = channelPosting.Amount.GetFee(fee, feeUnit);
                var vatAmount = feeAmount * (subMerchant.VatRate!.Value / 100);

                var netAmount = channelPosting.Amount - (feeAmount + vatAmount);
                var tip = journal.JournalDetails?.IncludedTip ?? 0.0M;
                var netTip = tip * netAmount / channelPosting.Amount;

                settlement.SettlementDetails.Upsert(journal.Id, detail =>
                {
                    detail.Amount = channelPosting.Amount;
                    detail.IncludedTip = tip;
                    detail.MerchantIban = subMerchant.Iban!;
                    detail.MerchantVatRate = subMerchant.VatRate.Value;
                    detail.TransactionFee = fee;
                    detail.FeeAmount = Math.Round(feeAmount, 2, MidpointRounding.AwayFromZero);
                    detail.VatAmount = Math.Round(vatAmount, 2, MidpointRounding.AwayFromZero);
                    detail.NetAmount = Math.Round(netAmount, 2, MidpointRounding.AwayFromZero);
                    detail.IncludedNetTip = Math.Round(netTip, 2, MidpointRounding.AwayFromZero);
                    detail.ParentMerchantId = channel.ParentMerchantId ?? throw new Exception("This should never happen");
                    detail.MerchantId = channel.MerchantId ?? throw new Exception("This should never happen");
                });
            }
        }

        private async Task ProcessSettlementServiceDetails(IUpdatableSettlement settlement, DateTime now)
        {
            if (settlement.SettlementDetails.Any() == false)
                return;

            var settlementDetailsPerSubMerchant = settlement.SettlementDetails.GroupBy(sd => sd.MerchantId).ToDictionary(sd => sd.Key, sd => sd.ToList());
            var merchantIds = settlement.SettlementDetails.Select(d => d.MerchantId).Distinct();

            var merchantAccounts = await queryProcessor.Execute(new GetPeopleAsyncQuery
            {
                MerchantIds = merchantIds,
                IncludePostings = true,
            });

            var quivi = await queryProcessor.Execute(new GetQuiviPersonAsyncQuery());
            var feesAccounts = await GetFeeAccounts(settlementDetailsPerSubMerchant);

            var unpaidServices = await queryProcessor.Execute(new GetMerchantServicesAsyncQuery
            {
                MerchantIds = merchantIds,
                UnpaidOnly = true,
                IncludeMerchants = true,
                IncludePostings = true,
            });

            var unpaidBillingServices = unpaidServices.Where(r => r.Type == MerchantServiceType.Billing).ToList();
            var unpaidBillingServicesDictionary = unpaidBillingServices.GroupBy(s => s.MerchantId).ToDictionary(s => s.Key, s => s.ToList());
            var unpaidReimbursementServices = unpaidServices.Where(r => r.Type == MerchantServiceType.Reimbursement).ToList();
            var unpaidReimbursementServicesDictionary = unpaidReimbursementServices.GroupBy(s => s.MerchantId).ToDictionary(s => s.Key, s => s.ToList());

            Func<int, string> comissionRefFunc = (int merchantId) => $"{merchantId}-QVFEE-{settlement.Date:yyyy-MM-dd}";
            Func<int, string> accountingClosingRefFunc = (int merchantId) => $"{merchantId}-QVCLS-{settlement.Date:yyyy-MM-dd}";
            Func<int, string> servicesRefFunc = (int serviceId) => $"{serviceId}-{settlement.Date:yyyy-MM-dd}";
            Func<int, string> servicesTaxRefFunc = (int serviceId) => $"{serviceId}-Vat-{settlement.Date:yyyy-MM-dd}";

            var orderRefs = merchantAccounts.SelectMany(a => new[]
            {
                comissionRefFunc(a.MerchantId!.Value),
                accountingClosingRefFunc(a.MerchantId!.Value),
            }).Concat(unpaidBillingServices.SelectMany(s => new[]
            {
                servicesRefFunc(s.Id),
                servicesTaxRefFunc(s.Id),
            }));

            var processedJournalsQuery = await queryProcessor.Execute(new GetJournalsAsyncQuery
            {
                OrderRefs = orderRefs,
            });
            var processedJournals = processedJournalsQuery.Select(r1 => r1.OrderRef!).ToHashSet();

            var addedJournals = new List<Journal>();
            var addedServices = new Dictionary<int, (Person Account, Journal? VatJournal, Journal? ServiceJournal, MerchantService MerchantService)>();
            foreach (var account in merchantAccounts)
            {
                var merchantId = account.MerchantId!.Value;

                var comissionRef = comissionRefFunc(merchantId);
                var accountingClosingRef = accountingClosingRefFunc(merchantId);

                if (settlementDetailsPerSubMerchant.TryGetValue(merchantId, out var details) == false)
                    continue;

                var totals = details.Aggregate((0.0M, 0.0M), (s, sd) => (s.Item1 + sd.Amount, s.Item2 + sd.NetAmount));
                decimal merchantProfit = totals.Item2;
                decimal totalFees = totals.Item1 - merchantProfit;
                if (totalFees != 0 && AlreadyProcessed(processedJournals, comissionRef) == false)
                    AddJournal(addedJournals, new Journal
                    {
                        State = JournalState.Completed,
                        Type = JournalType.TransactionFees,
                        OrderRef = comissionRef,
                        Method = JournalMethod.Integrated,
                        ChargeMethod = null,
                        JournalDetails = null,

                        Postings =
                        [
                            new Posting
                            {
                                Amount = totalFees * -1,
                                PersonId = account.Id,
                                AssetType = "EUR",

                                CreatedDate = now,
                                ModifiedDate = now,
                            },
                            new Posting
                            {
                                Amount = totalFees,
                                PersonId = feesAccounts[merchantId].Id,
                                AssetType = "EUR",

                                CreatedDate = now,
                                ModifiedDate = now,
                            },
                        ],
                        JournalChanges =
                        [
                            new JournalChange
                            {
                                State = JournalState.Completed,
                                Type = JournalType.TransactionFees,
                                Amount = totalFees,

                                CreatedDate = now,
                                ModifiedDate = now,
                            },
                        ],

                        CreatedDate = now,
                    });

                if (merchantProfit > 0 && AlreadyProcessed(processedJournals, accountingClosingRef) == false)
                    AddJournal(addedJournals, new Journal
                    {
                        State = JournalState.Completed,
                        Type = JournalType.AccountingClosing,
                        OrderRef = accountingClosingRef,
                        Method = JournalMethod.Integrated,
                        ChargeMethod = null,
                        JournalDetails = null,

                        Postings =
                        [
                            new Posting
                            {
                                Amount = merchantProfit * -1,
                                PersonId = account.Id,
                                AssetType = "EUR",

                                CreatedDate = now,
                                ModifiedDate = now,
                            },
                            new Posting
                            {
                                Amount = merchantProfit,
                                PersonId = quivi.Id,
                                AssetType = "EUR",

                                CreatedDate = now,
                                ModifiedDate = now,
                            },
                        ],
                        JournalChanges =
                        [
                            new JournalChange
                            {
                                State = JournalState.Completed,
                                Type = JournalType.AccountingClosing,
                                Amount = merchantProfit,

                                CreatedDate = now,
                                ModifiedDate = now,
                            },
                        ],

                        CreatedDate = now,
                    });

                if (merchantProfit > 0 && unpaidReimbursementServicesDictionary.TryGetValue(merchantId, out var reimbursementServices) == true)
                    foreach (var service in reimbursementServices)
                    {
                        var serviceRef = servicesRefFunc(service.Id);
                        if (AlreadyProcessed(processedJournals, serviceRef) == false)
                        {
                            var reimbursingTotal = service.Person!.Postings!.Sum(s => s.Amount);

                            var reimbursementCommand = new Journal
                            {
                                State = JournalState.Completed,
                                Type = JournalType.MerchantReimbursement,
                                OrderRef = serviceRef,
                                Method = JournalMethod.Integrated,
                                ChargeMethod = null,
                                JournalDetails = null,

                                Postings =
                                [
                                    new Posting
                                    {
                                        Amount = reimbursingTotal * -1,
                                        PersonId = service.PersonId,
                                        AssetType = "EUR",

                                        CreatedDate = now,
                                        ModifiedDate = now,
                                    },
                                    new Posting
                                    {
                                        Amount = reimbursingTotal,
                                        PersonId = account.Id,
                                        AssetType = "EUR",

                                        CreatedDate = now,
                                        ModifiedDate = now,
                                    },
                                ],
                                JournalChanges =
                                [
                                    new JournalChange
                                    {
                                        State = JournalState.Completed,
                                        Type = JournalType.MerchantReimbursement,
                                        Amount = reimbursingTotal,

                                        CreatedDate = now,
                                        ModifiedDate = now,
                                    },
                                ],

                                CreatedDate = now,
                            };
                            AddJournal(addedJournals, reimbursementCommand);

                            addedServices.Add(service.PersonId, (account, null, reimbursementCommand, service));
                            merchantProfit -= reimbursingTotal;
                        }
                    }

                if (merchantProfit > 0 && unpaidBillingServicesDictionary.TryGetValue(merchantId, out var billingServices) == true)
                    foreach (var service in billingServices)
                    {
                        var serviceRef = servicesRefFunc(service.Id);
                        var serviceTaxRef = servicesTaxRefFunc(service.Id);
                        if (AlreadyProcessed(processedJournals, serviceRef) == false)
                        {
                            var remainingToPayWithoutTax = service.Person!.Postings!.Sum(s => s.Amount);
                            var merchantTaxRate = (service.Merchant.VatRate ?? 0) / 100.0M;
                            decimal tax = Math.Round(remainingToPayWithoutTax * merchantTaxRate, 2, MidpointRounding.ToEven);
                            decimal remainingToPay = remainingToPayWithoutTax + tax;
                            var payingTotal = remainingToPay > merchantProfit ? merchantProfit : remainingToPay;
                            var payingTax = Math.Round(payingTotal - payingTotal / (1.0M + merchantTaxRate), 2, MidpointRounding.ToEven);

                            var serviceVatCommand = new Journal
                            {
                                State = JournalState.Completed,
                                Type = JournalType.MerchantBillingVat,
                                OrderRef = serviceTaxRef,
                                Method = JournalMethod.Integrated,
                                ChargeMethod = null,
                                JournalDetails = null,

                                Postings =
                                [
                                    new Posting
                                    {
                                        Amount = payingTax * -1,
                                        PersonId = account.Id,
                                        AssetType = "EUR",

                                        CreatedDate = now,
                                        ModifiedDate = now,
                                    },
                                    new Posting
                                    {
                                        Amount = payingTax,
                                        PersonId = service.PersonId,
                                        AssetType = "EUR",

                                        CreatedDate = now,
                                        ModifiedDate = now,
                                    },
                                ],
                                JournalChanges =
                                [
                                    new JournalChange
                                    {
                                        State = JournalState.Completed,
                                        Type = JournalType.MerchantBillingVat,
                                        Amount = payingTax,

                                        CreatedDate = now,
                                        ModifiedDate = now,
                                    },
                                ],

                                CreatedDate = now,
                            };
                            var personToServiceCommand = new Journal
                            {
                                State = JournalState.Completed,
                                Type = JournalType.MerchantBilling,
                                OrderRef = serviceRef,
                                Method = JournalMethod.Integrated,
                                ChargeMethod = null,
                                JournalDetails = null,

                                Postings =
                                [
                                    new Posting
                                    {
                                        Amount = payingTotal * -1,
                                        PersonId = service.PersonId,
                                        AssetType = "EUR",

                                        CreatedDate = now,
                                        ModifiedDate = now,
                                    },
                                    new Posting
                                    {
                                        Amount = payingTotal,
                                        PersonId = account.Id,
                                        AssetType = "EUR",

                                        CreatedDate = now,
                                        ModifiedDate = now,
                                    },
                                ],
                                JournalChanges =
                                [
                                    new JournalChange
                                    {
                                        State = JournalState.Completed,
                                        Type = JournalType.MerchantBilling,
                                        Amount = payingTotal,

                                        CreatedDate = now,
                                        ModifiedDate = now,
                                    },
                                ],

                                CreatedDate = now,
                            };
                            AddJournal(addedJournals, serviceVatCommand);
                            AddJournal(addedJournals, personToServiceCommand);

                            addedServices.Add(service.PersonId, (account, serviceVatCommand, personToServiceCommand, service));
                            merchantProfit -= payingTotal;
                        }

                        if (merchantProfit == 0)
                            break;

                        if (merchantProfit < 0)
                            throw new Exception($"Merchant {merchantId} has a negative amount to be paid. This should never happen.");
                    }
            }

            if (addedJournals.Any() == false)
                return;

            var journalsDictionary = addedJournals.ToDictionary(j => j.OrderRef!, j => j);

            foreach (var a in addedServices)
            {
                var serviceId = a.Key;
                var orderRef = servicesRefFunc(serviceId);
                var orderTaxRef = servicesTaxRefFunc(serviceId);

                var merchantAccount = a.Value.Account;
                var taxJournal = a.Value.VatJournal;
                var serviceJournal = a.Value.ServiceJournal;
                var service = a.Value.MerchantService;
                var settlementDetail = settlementDetailsPerSubMerchant[merchantAccount.MerchantId!.Value].First();

                var totalAmount = serviceJournal?.JournalChanges!.First().Amount ?? 0m;
                var taxAmount = taxJournal?.JournalChanges!.First().Amount ?? 0m;
                var journalService = journalsDictionary[orderRef];

                settlement.SettlementServiceDetails.Upsert(journalService.Id, details =>
                {
                    details.MerchantServiceId = service.Id;

                    details.ParentMerchantId = merchantAccount.ParentMerchantId!.Value;
                    details.MerchantId = merchantAccount.MerchantId.Value;
                    details.MerchantIban = settlementDetail.MerchantIban;
                    details.MerchantVatRate = settlementDetail.MerchantVatRate;

                    details.Amount = -(totalAmount - taxAmount);
                    details.VatAmount = -taxAmount;
                });
            }
        }

        private void AddJournal(ICollection<Journal> journalList, Journal journal)
        {
            journalList.Add(journal);
            journalsRepository.Add(journal);
        }

        private async Task<IReadOnlyDictionary<int, Person>> GetFeeAccounts(Dictionary<int, List<IUpdatableSettlementDetail>> settlementDetailsPerSubMerchant)
        {
            var submerchantPairs = settlementDetailsPerSubMerchant.Select(r => new UpsertSubmerchantFeesAccount
            {
                MerchantId = r.Key,
                ParentMerchantId = r.Value.First().ParentMerchantId
            });

            var aux = await commandProcessor.Execute(new UpsertSubMerchantFeesAccountAsyncCommand
            {
                SubmerchantFeesAccounts = submerchantPairs
            });
            return aux.ToDictionary(r1 => r1.MerchantId!.Value, r1 => r1);
        }

        private bool AlreadyProcessed(HashSet<string> dictionary, string reference) => dictionary.Contains(reference);
    }
}
