using Quivi.Application.Commands.PosChargeSyncAttempts;
using Quivi.Application.Commands.Sessions;
using Quivi.Application.Pos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Pos
{
    public class ProcessQuiviSyncChargeAsyncCommand : ICommand<Task<IEnumerable<IEvent>>>
    {
        public int PosChargeId { get; init; }
        public required AQuiviSyncStrategy SyncStrategy { get; init; }
    }

    public class ProcessQuiviSyncChargeAsyncCommandHandler : AProcessChargeAsyncCommandHandler<ProcessQuiviSyncChargeAsyncCommand>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IPosChargesRepository posChargesRepository;

        public ProcessQuiviSyncChargeAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                            ICommandProcessor commandProcessor,
                                                            IPosChargesRepository posChargesRepository,
                                                            IBackgroundJobHandler backgroundJobHandler,
                                                            IDateTimeProvider dateTimeProvider) : base(dateTimeProvider, backgroundJobHandler)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.posChargesRepository = posChargesRepository;
        }

        protected override Task Sync(ProcessQuiviSyncChargeAsyncCommand command)
        {
            return commandProcessor.Execute(new UpsertPosChargeSyncAttemptAsyncCommand
            {
                Criteria = new GetPosChargeSyncAttemptsCriteria
                {
                    PosChargeIds = [command.PosChargeId],
                    States = [SyncAttemptState.Syncing, SyncAttemptState.Synced],
                    Types = [SyncAttemptType.Payment],
                    PageSize = 1,
                },
                UpdateAction = async e =>
                {
                    if (e.State != SyncAttemptState.Syncing)
                        return;

                    var posChargesQuery = await posChargesRepository.GetAsync(new GetPosChargesCriteria
                    {
                        Ids = [command.PosChargeId],
                        IncludePosChargeSelectedMenuItems = true,
                    });

                    var posCharge = posChargesQuery.Single();
                    if (posCharge.CaptureDate.HasValue == false)
                        throw new Exception($"Trying to process an invoice for an incompleted PosCharge with Id {command.PosChargeId}");

                    decimal paymentAmount = await ProcessPayment(command, posCharge);

                    e.State = SyncAttemptState.Synced;
                    e.SyncedAmount = paymentAmount;
                },
            });
        }

        private async Task<decimal> ProcessPayment(ProcessQuiviSyncChargeAsyncCommand command, PosCharge posCharge)
        {
            Session session = await GetSession(posCharge);
            return await base.ProcessPayment(command.SyncStrategy, session, posCharge, posChargesRepository.SaveChangesAsync);
        }

        private async Task<Session> GetSession(PosCharge posCharge)
        {
            var sessionsQuery = await queryProcessor.Execute(new GetSessionsAsyncQuery
            {
                Ids = [posCharge.SessionId!.Value],
                IncludeOrdersMenuItemsPosChargeInvoiceItems = true,
                IncludeOrdersMenuItemsModifiers = true,
                PageSize = 1,
            });
            return sessionsQuery.Single();
        }
    }
}