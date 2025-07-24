using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Commands.Charges
{
    public class ProcessChargeAsyncCommand : ICommand<Task<Charge?>>
    {
        public int Id { get; init; }
        public string? PhoneNumber { get; init; }
    }

    public class ProcessChargeAsyncCommandHandler : ICommandHandler<ProcessChargeAsyncCommand, Task<Charge?>>
    {
        private readonly IIdConverter idConverter;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public ProcessChargeAsyncCommandHandler(IUnitOfWork unitOfWork,
                                                IDateTimeProvider dateTimeProvider,
                                                IIdConverter idConverter,
                                                IEventService eventService)
        {
            this.unitOfWork = unitOfWork;
            this.dateTimeProvider = dateTimeProvider;
            this.idConverter = idConverter;
            this.eventService = eventService;
        }
        public async Task<Charge?> Handle(ProcessChargeAsyncCommand command)
        {
            await using var transaction = await unitOfWork.StartTransactionAsync();
            Charge? charge = await ProcessCharge(command);
            if(charge == null)
            {
                await transaction.RollbackAsync();
                return null;
            }
            await transaction.CommitAsync();

            if(charge.IsTopUp())
            {
                //TODO: Send Top Up Event
                return charge;
            }

            await eventService.Publish(new OnPosChargeOperationEvent
            {
                Id = charge.Id,
                MerchantId = charge.PosCharge!.MerchantId,
                ChannelId = charge.PosCharge!.ChannelId,
                Operation = EntityOperation.Update,
            });

            await eventService.Publish(new OnPosChargeCapturedEvent
            {
                Id = charge.Id,
                MerchantId = charge.PosCharge!.MerchantId,
                ChannelId = charge.PosCharge!.ChannelId,
            });

            return charge;
        }

        private async Task<Charge?> ProcessCharge(ProcessChargeAsyncCommand command)
        {
            var chargeQuery = await unitOfWork.Charges.GetAsync(new GetChargesCriteria
            {
                Ids = [command.Id],
                IncludeDeposit = true,
                IncludeDepositDepositCapture = true,
                IncludeDepositCaptureJournal = true,
                IncludeDepositDepositSurcharge = true,
                IncludeDepositSurchargeJournal = true,
                IncludePosCharge = true,
                IncludeDepositConsumer = true,
                IncludeChainedCharge = true,
                IncludeDepositDepositCapturePerson = true,
            });
            var charge = chargeQuery.SingleOrDefault();
            if(charge == null)
                return null;

            if (charge.Status != ChargeStatus.Processing)
                return null;

            if (charge.Deposit!.DepositCaptureJournal != null)
                return null;

            if (charge.Deposit!.DepositSurchargeJournal != null)
                return null;

            var quiviPersonQuery = await unitOfWork.People.GetAsync(new GetPeopleCriteria
            {
                PersonTypes = [PersonType.Quivi],
                PageSize = 1,
            });
            var quiviPerson = quiviPersonQuery.Single();

            var tip = charge.PosCharge!.Tip;
            var now = dateTimeProvider.GetUtcNow();

            ProcessConsumerJournal(charge, quiviPerson, tip, now);
            ProcessCaptureJournal(charge, charge.Deposit!.DepositCapture?.Amount ?? 0, tip, now);
            ProcessSurchargeJournal(quiviPerson, charge, charge.Deposit!.DepositSurchage?.Amount ?? 0.0M, now);

            charge.PosCharge!.PhoneNumber ??= command.PhoneNumber;
            charge.PosCharge!.CaptureDate = now;
            charge.Status = ChargeStatus.Completed;

            await unitOfWork.SaveChangesAsync();
            return charge;
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
                        PersonId = charge.Deposit.Consumer!.Id,

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
    }
}