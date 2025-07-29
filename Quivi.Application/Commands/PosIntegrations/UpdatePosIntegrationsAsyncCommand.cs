using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosIntegrations;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PosIntegrations
{
    public interface IUpdatablePosIntegration : IUpdatableEntity
    {
        int Id { get; }
        int MerchantId { get; }
        SyncState SyncState { get; set; }
        string ConnectionString { get; set; }
    }

    public class UpdatePosIntegrationsAsyncCommand : AUpdateAsyncCommand<IEnumerable<PosIntegration>, IUpdatablePosIntegration>
    {
        public required GetPosIntegrationsCriteria Criteria { get; init; }
    }

    public class UpdatePosIntegrationsAsyncCommandHandler : ICommandHandler<UpdatePosIntegrationsAsyncCommand, Task<IEnumerable<PosIntegration>>>
    {
        private readonly IPosIntegrationsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdatePosIntegrationsAsyncCommandHandler(IPosIntegrationsRepository repository,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<PosIntegration>> Handle(UpdatePosIntegrationsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria);
            if (entities.Any() == false)
                return [];

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatablePosIntegration> changedEntities = new();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatablePosIntegration(entity);
                await command.UpdateAction.Invoke(updatableEntity);
                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;

                    if (updatableEntity.SyncStateChanged)
                        entity.LastSyncingDate = now;

                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();

            foreach (var entity in changedEntities)
                await eventService.Publish(new OnPosIntegrationOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Update,
                });

            return entities;
        }

        private class UpdatablePosIntegration : IUpdatablePosIntegration
        {
            public PosIntegration Model { get; }
            private readonly SyncState originalSyncState;
            private readonly string originalConnectionString;

            public UpdatablePosIntegration(PosIntegration model)
            {
                Model = model;
                originalSyncState = model.SyncState;
                originalConnectionString = model.ConnectionString;
            }

            public int Id => this.Model.Id;
            public int MerchantId => this.Model.MerchantId;
            public SyncState SyncState
            {
                get => Model.SyncState;
                set => Model.SyncState = value;
            }

            public string ConnectionString
            {
                get => Model.ConnectionString;
                set => Model.ConnectionString = value;
            }

            public bool SyncStateChanged => originalSyncState != Model.SyncState;
            public bool HasChanges
            {
                get
                {
                    if (SyncStateChanged)
                        return true;

                    if (originalConnectionString != Model.ConnectionString)
                        return true;

                    return false;
                }
            }
        }
    }
}
