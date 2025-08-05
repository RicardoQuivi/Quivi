using Quivi.Application.Queries.People;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Abstractions.Services.Charges.Results;
using Quivi.Infrastructure.Extensions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Quivi.Application.Services.Acquirers
{
    public class AcquirerChargeStatusProcessor
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IChargesRepository chargesRepository;

        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEnumerable<IAcquirerProcessor> acquirerStrategies;
        private readonly IEventService eventService;
        private readonly IBackgroundJobHandler backgroundJobHandler;
        private readonly IIdConverter idConverter;
        private readonly IQueryProcessor queryProcessor;

        public AcquirerChargeStatusProcessor(IUnitOfWork unitOfWork,
                                        IEnumerable<IAcquirerProcessor> acquirerStrategies,
                                        IDateTimeProvider dateTimeProvider,
                                        IEventService eventService,
                                        IBackgroundJobHandler backgroundJobHandler,
                                        IIdConverter idConverter,
                                        IQueryProcessor queryProcessor)
        {
            this.unitOfWork = unitOfWork;
            chargesRepository = unitOfWork.Charges;

            this.acquirerStrategies = acquirerStrategies;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
            this.backgroundJobHandler = backgroundJobHandler;
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
        }

        public async Task<StartProcessingResult> ProcessAsync(int chargeId, Func<object?> onStartProcessingContext)
        {
            string? challengeUrl = null;
            var charge = await ProcessChargeWithTransaction(chargeId, async (c) =>
            {
                bool hasChanges = false;

                if (c.Status == ChargeStatus.Requested)
                {
                    challengeUrl = await ProcessCharge(c, onStartProcessingContext);
                    hasChanges = true;
                }

                if (c.Status == ChargeStatus.Processing)
                {
                    var status = await CheckAndUpdateState(c);
                    if (status == PaymentStatus.Failed || status == PaymentStatus.Success)
                        hasChanges = true;
                }

                return hasChanges;
            });

            if (charge == null)
                return new StartProcessingResult();

            if (charge.Status == ChargeStatus.Processing)
                SchedulePolling(charge.Id);

            return new StartProcessingResult
            {
                Charge = charge,
                ChallengeUrl = challengeUrl,
            };
        }

        public Task CheckAndUpdateStateAsync(int chargeId) => Polling(chargeId, false);

        private async Task<Charge?> GetCharge(int chargeId)
        {
            var chargesQuery = await chargesRepository.GetAsync(new GetChargesCriteria
            {
                Ids = [chargeId],

                IncludePosCharge = true,
                IncludeAcquirerCharge = true,
                IncludeMerchantAcquirerConfiguration = true,

                IncludeDeposit = true,
                IncludeDepositDepositSurchage = true,
                IncludeDepositDepositCapture = true,
                IncludeDepositCaptureJournal = true,
                IncludeDepositDepositSurcharge = true,
                IncludeDepositSurchargeJournal = true,
                IncludeDepositConsumer = true,
                IncludeChainedCharge = true,
                IncludeDepositDepositCapturePerson = true,
            });
            var charge = chargesQuery.SingleOrDefault();
            return charge;
        }

        private async Task<Charge?> ProcessChargeWithTransaction(int chargeId, Func<Charge, Task<bool>> action)
        {
            var charge = await GetCharge(chargeId);
            if (charge == null)
                return null;

            await using var transaction = await unitOfWork.StartTransactionAsync();

            var hasChanges = await action(charge);
            if (hasChanges == false)
            {
                await transaction.RollbackAsync();
                return charge;
            }

            await chargesRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            await eventService.Publish(new OnPosChargeOperationEvent
            {
                Id = charge.PosCharge!.Id,
                ChannelId = charge.PosCharge!.ChannelId,
                MerchantId = charge.PosCharge!.MerchantId,
                Operation = EntityOperation.Update,
            });

            if (charge.PosCharge.CaptureDate.HasValue)
                await eventService.Publish(new OnPosChargeCapturedEvent
                {
                    Id = charge.Id,
                    MerchantId = charge.PosCharge!.MerchantId,
                    ChannelId = charge.PosCharge!.ChannelId,
                });

            return charge;
        }

        private async Task<string?> ProcessCharge(Charge charge, Func<object?> onStartProcessingContext)
        {
            string? challengeUrl = null;

            var strategy = acquirerStrategies.FirstOrDefault(c => c.ChargePartner == charge.ChargePartner && c.ChargeMethod == charge.ChargeMethod);
            if (strategy == null)
            {
                charge.Status = ChargeStatus.Failed;
                charge.ModifiedDate = dateTimeProvider.GetUtcNow();
                return challengeUrl;
            }

            try
            {
                var context = onStartProcessingContext();
                charge.AcquirerCharge!.AdditionalJsonContext = MergeContext(charge.AcquirerCharge, context);
                var processingResult = await strategy.Process(charge);
                challengeUrl = processingResult.ChallengeUrl;
                charge.AcquirerCharge!.AdditionalJsonContext = MergeContext(charge.AcquirerCharge, processingResult);

                charge.Status = ChargeStatus.Processing;
                charge.ModifiedDate = dateTimeProvider.GetUtcNow();
            }
            catch
            {
                charge.Status = ChargeStatus.Failed;
                charge.ModifiedDate = dateTimeProvider.GetUtcNow();
            }
            return challengeUrl;
        }

        private async Task<PaymentStatus> CheckAndUpdateState(Charge charge)
        {
            var strategy = acquirerStrategies.FirstOrDefault(c => c.ChargePartner == charge.ChargePartner && c.ChargeMethod == charge.ChargeMethod);
            if (strategy == null)
            {
                charge.Status = ChargeStatus.Failed;
                charge.ModifiedDate = dateTimeProvider.GetUtcNow();
                return PaymentStatus.Failed;
            }

            var result = await strategy.GetStatus(charge);
            if (result.Status == PaymentStatus.Failed)
            {
                charge.Status = ChargeStatus.Failed;
                charge.ModifiedDate = dateTimeProvider.GetUtcNow();
                return PaymentStatus.Failed;
            }
            else if (result.Status == PaymentStatus.Success)
            {
                charge.AcquirerCharge!.AcquirerId ??= result.GatewayId;
                await ProcessAcquirerCompletedCharge(charge);
            }

            return result.Status;
        }

        public void SchedulePolling(int chargeId) => backgroundJobHandler.Schedule(() => Polling(chargeId, true), TimeSpan.FromSeconds(15));

        public async Task Polling(int chargeId, bool continuePolling)
        {
            try
            {
                await ProcessChargeWithTransaction(chargeId, async (c) =>
                {
                    var chargeInitialState = c.Status;
                    var status = await CheckAndUpdateState(c);
                    if (status != PaymentStatus.Processing)
                    {
                        continuePolling = false;
                        return true;
                    }

                    return false;
                });
            }
            catch
            {
            }

            if (continuePolling)
                SchedulePolling(chargeId);
        }

        #region Complete Charge
        private async Task ProcessAcquirerCompletedCharge(Charge charge)
        {
            if (charge.Status != ChargeStatus.Processing)
                return;

            if (charge.Deposit!.DepositCaptureJournal != null)
                return;

            if (charge.Deposit!.DepositSurchargeJournal != null)
                return;

            var quiviPerson = await GetQuiviPerson();

            var tip = charge.PosCharge!.Tip;
            var now = dateTimeProvider.GetUtcNow();

            ProcessConsumerJournal(charge, quiviPerson, tip, now);
            ProcessCaptureJournal(charge, charge.Deposit!.DepositCapture?.Amount ?? 0, tip, now);
            ProcessSurchargeJournal(quiviPerson, charge, charge.Deposit!.DepositSurchage?.Amount ?? 0.0M, now);

            //TODO: Should I have this?
            //if (charge.ChargeMethod == ChargeMethod.MbWay && charge.MbWayCharge != null)
            //    charge.PosCharge!.PhoneNumber ??= charge.MbWayCharge.PhoneNumber;

            charge.PosCharge!.CaptureDate = now;
            charge.Status = ChargeStatus.Completed;
        }

        private void ProcessConsumerJournal(Charge charge, Person quiviPerson, decimal tip, DateTime now)
        {
            if (charge.ChargeMethod == ChargeMethod.Wallet || charge.Deposit!.DepositCapture!.Amount <= 0)
                return;

            var amount = charge.Deposit.Amount;
            var journal = new Journal
            {
                State = JournalState.Completed,
                Type = JournalType.Deposit,
                OrderRef = idConverter.ToPublicId(charge.Id),
                JournalDetails = tip <= 0.0M ? null : new JournalDetails
                {
                    IncludedTip = tip,
                    CreatedDate = now,
                    ModifiedDate = now,
                },
                Postings = new List<Posting>
                {
                    new Posting
                    {
                        Amount = amount,
                        AssetType = "EUR",
                        PersonId = charge.Deposit.ConsumerId,

                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                    new Posting
                    {
                        Amount = amount * -1,
                        AssetType = "EUR",
                        PersonId = quiviPerson.Id,

                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                },
                DepositJournals = new List<DepositJournal>
                {
                    new DepositJournal
                    {
                        DepositId = charge.Id,
                    },
                },
                CreatedDate = now,
                ModifiedDate = now,
            };
            journal.JournalChanges = new List<JournalChange>
            {
                new JournalChange
                {
                    State = JournalState.Completed,
                    Type = JournalType.Deposit,
                    Amount = amount,

                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                }
            };
            unitOfWork.Journals.Add(journal);
        }

        private void ProcessCaptureJournal(Charge charge, decimal captureAmount, decimal tip, DateTime now)
        {
            if (charge.IsTopUp())
                return;

            if (captureAmount == 0)
                return;

            var journal = new Journal
            {
                State = JournalState.Completed,
                Type = charge.Deposit!.DepositCapture!.Type,
                OrderRef = idConverter.ToPublicId(charge.Id),
                Method = JournalMethod.Integrated,
                ChargeMethod = charge.ChargeMethod,
                JournalDetails = tip > 0.0M ? new JournalDetails
                {
                    IncludedTip = tip,
                } : null,
                Postings = new List<Posting>
                {
                    new Posting
                    {
                        Amount = captureAmount * -1,
                        AssetType = "EUR",
                        PersonId = charge.Deposit.ConsumerId,

                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                    new Posting
                    {
                        Amount = captureAmount,
                        AssetType = "EUR",
                        PersonId = charge.Deposit.DepositCapture.PersonId,

                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                },
                DepositCaptureJournals = new List<DepositCaptureJournal>
                {
                    new DepositCaptureJournal
                    {
                        DepositId = charge.Id,
                    },
                },
                CreatedDate = now,
                ModifiedDate = now,
            };
            journal.JournalChanges = new List<JournalChange>
            {
                new JournalChange
                {
                    State = JournalState.Completed,
                    Type = charge.Deposit!.DepositCapture!.Type,
                    Amount = captureAmount,

                    Journal = journal,

                    CreatedDate = now,
                    ModifiedDate = now,
                },
            };
            unitOfWork.Journals.Add(journal);
        }

        private void ProcessSurchargeJournal(Person quiviPerson, Charge charge, decimal surcharge, DateTime now)
        {
            if (charge.IsTopUp())
                return;

            if (surcharge <= 0)
                return;

            var journal = new Journal
            {
                State = JournalState.Completed,
                Type = JournalType.Surcharge,
                OrderRef = idConverter.ToPublicId(charge.Id),
                Method = JournalMethod.Integrated,
                ChargeMethod = charge.ChargeMethod,
                Postings = new List<Posting>
                {
                    new Posting
                    {
                        Amount = surcharge * -1,
                        AssetType = "EUR",
                        PersonId = charge.Deposit!.ConsumerId,

                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                    new Posting
                    {
                        Amount = surcharge,
                        AssetType = "EUR",
                        PersonId = quiviPerson.Id,

                        CreatedDate = now,
                        ModifiedDate = now,
                    }
                },
                DepositSurchargeJournals = new List<DepositSurchargeJournal>
                {
                    new DepositSurchargeJournal
                    {
                        DepositId = charge.Id,
                    },
                },
                CreatedDate = now,
                ModifiedDate = now,
            };
            journal.JournalChanges = new List<JournalChange>
            {
                new JournalChange
                {
                    State = JournalState.Completed,
                    Type = JournalType.Surcharge,
                    Amount = surcharge,

                    CreatedDate = now,
                    ModifiedDate = now,

                    Journal = journal,
                }
            };
            unitOfWork.Journals.Add(journal);
        }

        private Task<Person> GetQuiviPerson() => queryProcessor.Execute(new GetQuiviPersonAsyncQuery());
        #endregion

        private static string MergeContext(AcquirerCharge acquirerCharge, object? context)
        {
            string? existingJson = acquirerCharge.AdditionalJsonContext;

            JsonObject existingJsonObj = string.IsNullOrWhiteSpace(existingJson) ? new JsonObject() : JsonNode.Parse(existingJson)?.AsObject() ?? new JsonObject();
            JsonObject newJsonObj = JsonSerializer.SerializeToNode(context)?.AsObject() ?? new JsonObject();

            foreach (var kvp in newJsonObj)
                existingJsonObj[kvp.Key] = kvp.Value?.Deserialize<JsonNode>();

            return existingJsonObj.ToJsonString();
        }
    }
}