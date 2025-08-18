using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosChargeSyncAttempts;
using Quivi.Infrastructure.Abstractions.Events.Data.Sessions;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PosChargeSyncAttempts
{
    public interface IUpdatablePosChargeSyncAttempt : IUpdatableEntity
    {
        int Id { get; }
        int PosChargeId { get; }
        SyncAttemptState State { get; set; }
        SyncAttemptType Type { get; }
        decimal SyncedAmount { get; set; }
    }

    public class UpsertPosChargeSyncAttemptAsyncCommand : AUpdateAsyncCommand<IEnumerable<PosChargeSyncAttempt>, IUpdatablePosChargeSyncAttempt>
    {
        public class OnCreateData
        {
            public required int PosChargeId { get; init; }
            public required int MerchantId { get; init; }
            public required SyncAttemptType Type { get; init; }
        }

        public required GetPosChargeSyncAttemptsCriteria Criteria { get; init; }
        public OnCreateData? CreateData { get; init; }
    }

    public class UpsertPosChargeSyncAttemptAsyncCommandHandler : ICommandHandler<UpsertPosChargeSyncAttemptAsyncCommand, Task<IEnumerable<PosChargeSyncAttempt>>>
    {
        private readonly IPosChargeSyncAttemptsRepository repository;
        private readonly IPosChargesRepository posChargeRepository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpsertPosChargeSyncAttemptAsyncCommandHandler(IPosChargeSyncAttemptsRepository repository,
                                                                IPosChargesRepository posChargeRepository,
                                                                IDateTimeProvider dateTimeProvider,
                                                                IEventService eventService)
        {
            this.repository = repository;
            this.posChargeRepository = posChargeRepository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<PosChargeSyncAttempt>> Handle(UpsertPosChargeSyncAttemptAsyncCommand command)
        {
            var entitiesQuery = await repository.GetAsync(command.Criteria with
            {
                IncludePosCharge = true,
            });

            PosChargeSyncAttempt? newEntity = null;
            var now = dateTimeProvider.GetUtcNow();
            var entities = entitiesQuery.ToList();
            if (entities.Any() == false && command.CreateData != null)
            {
                var posChargeQuery = await posChargeRepository.GetAsync(new GetPosChargesCriteria
                {
                    Ids = [command.CreateData.PosChargeId],
                    MerchantIds = [command.CreateData.MerchantId],
                    PageIndex = 0,
                    PageSize = 1,
                });
                var posCharge = posChargeQuery.Single();
                newEntity = new PosChargeSyncAttempt
                {
                    State = SyncAttemptState.Syncing,
                    Type = command.CreateData.Type,
                    SyncedAmount = 0.0m,
                    PosChargeId = command.CreateData.PosChargeId,
                    PosCharge = posCharge,
                    CreatedDate = now,
                    ModifiedDate = now,
                };
                entities.Add(newEntity);
                repository.Add(newEntity);
            }

            List<UpdatablePosChargeSyncAttempt> changedEntities = new();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatablePosChargeSyncAttempt(entity, entity == newEntity);
                await command.UpdateAction.Invoke(updatableEntity);
                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();
            foreach (var entity in entities)
            {
                await eventService.Publish(new OnPosChargeSyncAttemptEvent
                {
                    Id = entity.Id,
                    PosChargeId = entity.PosChargeId,
                    MerchantId = entity.PosCharge?.MerchantId ?? command.CreateData!.MerchantId,
                    Operation = entity == newEntity ? EntityOperation.Create : EntityOperation.Update,
                });

                if (entity.PosCharge?.SessionId == null)
                    continue;

                await eventService.Publish(new OnSessionOperationEvent
                {
                    Id = entity.PosCharge.SessionId.Value,
                    ChannelId = entity.PosCharge.ChannelId,
                    MerchantId = entity.PosCharge?.MerchantId ?? command.CreateData!.MerchantId,
                    Operation = EntityOperation.Update,
                });
            }
            return entities;
        }

        private class UpdatablePosChargeSyncAttempt : IUpdatablePosChargeSyncAttempt
        {
            private PosChargeSyncAttempt Model { get; }
            private readonly bool isNew;
            private readonly SyncAttemptState originalState;
            private readonly decimal originalSyncedAmount;

            public UpdatablePosChargeSyncAttempt(PosChargeSyncAttempt model, bool isNew)
            {
                this.isNew = isNew;
                this.Model = model;
                this.originalState = this.Model.State;
                this.originalSyncedAmount = this.Model.SyncedAmount;
            }

            public int Id => Model.Id;
            public int PosChargeId => Model.PosChargeId;
            public SyncAttemptState State
            {
                get => Model.State;
                set => Model.State = value;
            }
            public decimal SyncedAmount
            {
                get => Model.SyncedAmount;
                set
                {
                    if (Model.State != SyncAttemptState.Synced)
                        throw new Exception("Cannot update synced amount if the state isn't synced");
                    Model.SyncedAmount = value;
                }
            }
            public SyncAttemptType Type => Model.Type;

            public DateTime CreatedDate => Model.CreatedDate;

            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    if (originalState != Model.State)
                        return true;

                    if (originalSyncedAmount != Model.SyncedAmount)
                        return true;

                    return false;
                }
            }
        }
    }
}