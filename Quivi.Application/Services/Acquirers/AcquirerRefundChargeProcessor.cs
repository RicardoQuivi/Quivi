using Quivi.Application.Queries.People;
using Quivi.Application.Queries.Postings;
using Quivi.Application.Services.Exceptions;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Abstractions.Services.Charges.Parameters;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Services.Acquirers
{
    public class AcquirerRefundChargeProcessor
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IChargesRepository chargesRepository;
        private readonly IJournalsRepository journalsRepository;

        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEnumerable<IAcquirerProcessor> acquirerStrategies;
        private readonly IEventService eventService;
        private readonly IIdConverter idConverter;
        private readonly IPosSyncService posSyncService;
        private readonly IQueryProcessor queryProcessor;

        public AcquirerRefundChargeProcessor(IUnitOfWork unitOfWork,
                                                IEnumerable<IAcquirerProcessor> acquirerStrategies,
                                                IDateTimeProvider dateTimeProvider,
                                                IEventService eventService,
                                                IIdConverter idConverter,
                                                IPosSyncService posSyncService,
                                                IQueryProcessor queryProcessor)
        {
            this.unitOfWork = unitOfWork;
            chargesRepository = unitOfWork.Charges;
            journalsRepository = unitOfWork.Journals;

            this.acquirerStrategies = acquirerStrategies;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
            this.idConverter = idConverter;
            this.posSyncService = posSyncService;
            this.queryProcessor = queryProcessor;
        }

        public async Task RefundAsync(RefundParameters parameters)
        {
            var validatedDetails = await GetAndValidateChargeForRefund(parameters);
            if (validatedDetails == null)
                return;

            var posCharge = await ProcessRefund(parameters, validatedDetails.Value.Charge, validatedDetails.Value.TotalRefundAmount, validatedDetails.Value.TipRefundAmount);
            await unitOfWork.SaveChangesAsync();

            await eventService.Publish(new OnPosChargeRefundedEvent
            {
                Id = posCharge.Id,
                ChannelId = posCharge.ChannelId,
                MerchantId = posCharge.MerchantId,
            });
        }

        private async Task<(Charge Charge, decimal TotalRefundAmount, decimal TipRefundAmount)?> GetAndValidateChargeForRefund(RefundParameters parameters)
        {
            if (parameters.Amount.HasValue && parameters.Amount.Value <= 0)
                return null;

            var chargesQuery = await chargesRepository.GetAsync(new GetChargesCriteria
            {
                MerchantIds = parameters.MerchantId.HasValue ? [parameters.MerchantId.Value] : null,
                Ids = [parameters.ChargeId],
                Statuses = [ChargeStatus.Completed],

                IncludePosCharge = true,
                IncludePosChargeMerchant = true,
                IncludeDeposit = true,
                IncludeDepositCaptureJournal = true,
                IncludeDepositCaptureJournalPostingsPerson = true,
                IncludeDepositCaptureJournalChanges = true,
                IncludeDepositDepositCapture = true,
                IncludeMerchantAcquirerConfiguration = true,
                IncludeAcquirerCharge = true,
            });

            var charge = chargesQuery.SingleOrDefault();
            if (charge == null)
                return null;

            if (charge.PosCharge!.PaymentRefund > 0)
                return null;

            var capturedAmount = charge.ChargeMethod == ChargeMethod.Custom ? charge.PosCharge.Total : charge.Deposit!.DepositCapture!.Amount;
            if (capturedAmount <= 0)
                return null;

            var availableAmount = capturedAmount;
            var refundAmount = parameters.Amount ?? availableAmount;

            if (charge.ChargeMethod != ChargeMethod.Custom)
            {
                var balance = await GetChannelBalanceSinceLastSettlement(charge.Deposit!.DepositCapture!.PersonId, charge.PosCharge.Merchant!);
                if (balance < refundAmount)
                    throw new NoBalanceException
                    {
                        CurrentBalance = balance,
                        RequiredBalance = refundAmount,
                    };
            }

            if (parameters.IsCancellation)
            {
                if (charge.PosCharge!.CaptureDate!.Value < dateTimeProvider.GetUtcNow().AddDays(-1))
                    throw new Exception(); //TODO: Throw a catchable Exception

                if (refundAmount != charge.PosCharge.Total)
                    throw new Exception(); //TODO: Throw a catchable Exception
            }

            if (!await posSyncService.CanRefundCharge(parameters.ChargeId, refundAmount, parameters.IsCancellation ? InvoiceRefundType.Cancellation : InvoiceRefundType.CreditNote))
                throw new NotSupportedException();

            var originalTip = charge.PosCharge.Tip;
            var originalPayment = charge.PosCharge.Payment;
            var tipRefund = originalTip > 0.00M && refundAmount > originalPayment ? refundAmount - originalPayment : 0;

            return (charge, refundAmount, tipRefund);
        }

        private async Task<PosCharge> ProcessRefund(RefundParameters parameters, Charge charge, decimal totalAmountToRefund, decimal tipRefundAmount)
        {
            var now = dateTimeProvider.GetUtcNow();
            if (charge.ChargeMethod != ChargeMethod.Custom)
            {
                var strategy = acquirerStrategies.FirstOrDefault(c => c.ChargePartner == charge.ChargePartner && c.ChargeMethod == charge.ChargeMethod);
                if (strategy == null)
                    throw new NotSupportedException();

                var originalJournal = charge.Deposit!.DepositCaptureJournal!.Journal!;
                ProcessMerchantJournal(charge, now, originalJournal, totalAmountToRefund, tipRefundAmount);
                await ProcessQuiviJournal(charge, now, totalAmountToRefund);
                await strategy.Refund(charge, totalAmountToRefund);
            }

            charge.PosCharge!.PaymentRefund = totalAmountToRefund - tipRefundAmount;
            charge.PosCharge!.TipRefund = tipRefundAmount;
            charge.PosCharge!.InvoiceRefundType = parameters.IsCancellation ? InvoiceRefundType.Cancellation : InvoiceRefundType.CreditNote;
            charge.PosCharge!.RefundReason = parameters.Reason;
            charge.PosCharge!.RefundEmployeeId = parameters.EmployeeId;
            charge.PosCharge!.TotalRefund = charge.PosCharge!.PaymentRefund + charge.PosCharge!.TipRefund;

            return charge.PosCharge;
        }

        private async Task<decimal> GetChannelBalanceSinceLastSettlement(int personId, Merchant merchant)
        {
            var fromDateUtc = dateTimeProvider.GetUtcNow().GetSettlementFromDateUtc(merchant.TimeZone);
            var balances = await queryProcessor.Execute(new GetAccountsBalanceAsyncQuery
            {
                PersonIds = [personId],
                FromDate = fromDateUtc,
                JournalTypes = Enum.GetValues(typeof(JournalType))
                                    .Cast<JournalType>()
                                    .Except(
                                    [
                                        JournalType.AccountingClosing,
                                        JournalType.TransactionFees,
                                        JournalType.MerchantBilling,
                                        JournalType.MerchantBillingVat,
                                        JournalType.MerchantReimbursement,
                                    ]),
            });

            return balances.DefaultIfEmpty()?.Single().Value ?? 0m;
        }

        private void ProcessMerchantJournal(Charge charge, DateTime now, Journal originalJournal, decimal refundAmount, decimal tipRefund)
        {
            var originalPostingConsumer = originalJournal.Postings!.First(p => p.Person!.PersonType == PersonType.Consumer);
            var originalPostingChannel = originalJournal.Postings!.First(p => p.Person!.PersonType == PersonType.Channel);

            var journal = new Journal
            {
                State = JournalState.Completed,
                Type = JournalType.Refund,
                OrderRef = originalJournal.OrderRef,
                Method = originalJournal.Method,
                JournalLinkId = originalJournal.Id,
                ChargeMethod = originalJournal.ChargeMethod,
                DepositRefundJournals = new List<DepositRefundJournal>
                {
                    new DepositRefundJournal
                    {
                        DepositId = charge.Id,

                        CreatedDate = now,
                        ModifiedDate = now,
                    }
                },
                JournalDetails = tipRefund > 0 ? new JournalDetails
                {
                    IncludedTip = tipRefund,

                    CreatedDate = now,
                    ModifiedDate = now,
                } : null,
                JournalChanges = new List<JournalChange>(),

                CreatedDate = now,
                ModifiedDate = now,
            };
            journal.Postings = new List<Posting>
            {
                new Posting
                {
                    Amount = refundAmount,
                    AssetType = originalPostingConsumer.AssetType,
                    PersonId = originalPostingConsumer.PersonId,
                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                },
                new Posting
                {
                    Amount = refundAmount * -1,
                    AssetType = originalPostingChannel.AssetType,
                    PersonId = originalPostingChannel.PersonId,
                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                },
            };

            journalsRepository.Add(journal);

            originalJournal.JournalChanges!.Add(new JournalChange
            {
                State = originalJournal.State,
                Type = originalJournal.Type,
                Amount = refundAmount,

                JournalLink = journal,
                JournalLinkId = journal.Id,

                JournalId = originalJournal.Id,
                Journal = originalJournal,

                CreatedDate = now,
                ModifiedDate = now,
            });

            journal.JournalChanges.Add(new JournalChange
            {
                State = journal.State,
                Type = journal.Type,
                Amount = refundAmount,

                JournalLink = originalJournal,
                JournalLinkId = originalJournal.Id,

                JournalId = journal.Id,
                Journal = journal,

                CreatedDate = now,
                ModifiedDate = now,
            });
        }

        private async Task ProcessQuiviJournal(Charge charge, DateTime now, decimal totalAmountToRefund)
        {
            var quiviPerson = await GetQuiviPerson();

            var journal = new Journal
            {
                State = JournalState.Completed,
                Type = JournalType.Withdrawal,
                OrderRef = idConverter.ToPublicId(charge.Id),
                JournalDetails = null,
                CreatedDate = now,
                ModifiedDate = now,
            };
            journal.Postings = new List<Posting>
            {
                new Posting
                {
                    Amount = totalAmountToRefund,
                    AssetType = "EUR",
                    PersonId = charge.Deposit!.Consumer!.Id,
                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                },
                new Posting
                {
                    Amount = totalAmountToRefund * -1,
                    AssetType = "EUR",
                    PersonId = quiviPerson.Id,
                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                },
            };

            journal.JournalChanges = new List<JournalChange>
            {
                new JournalChange
                {
                    CreatedDate = journal.CreatedDate,
                    State = journal.State,
                    Type = journal.Type,
                    Amount = totalAmountToRefund,
                    JournalLinkId = journal.JournalLinkId,
                    Journal = journal,
                }
            };

            journalsRepository.Add(journal);
        }

        private Task<Person> GetQuiviPerson() => queryProcessor.Execute(new GetQuiviPersonAsyncQuery());
    }
}