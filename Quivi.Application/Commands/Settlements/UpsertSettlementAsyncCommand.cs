using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Settlements;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Settlements
{
    public class UpsertSettlementAsyncCommand : AUpdateAsyncCommand<Settlement, IUpdatableSettlement>
    {
        public DateOnly Date { get; init; }
    }

    internal class UpsertSettlementAsyncCommandHandler : ICommandHandler<UpsertSettlementAsyncCommand, Task<Settlement>>
    {
        private readonly ISettlementsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpsertSettlementAsyncCommandHandler(ISettlementsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<Settlement> Handle(UpsertSettlementAsyncCommand command)
        {
            var entities = await repository.GetAsync(new GetSettlementsCriteria
            {
                Dates = [command.Date],
                IncludeSettlementDetails = true,
                IncludeSettlementServiceDetails = true,

                PageIndex = 0,
                PageSize = 1,
            });
            var now = dateTimeProvider.GetUtcNow();
            var entity = entities.SingleOrDefault();

            if (entity == null)
            {
                entity = new Settlement
                {
                    Date = command.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    State = SettlementState.Processing,

                    SettlementDetails = [],
                    SettlementServiceDetails = [],

                    CreatedDate = now,
                    ModifiedDate = now,
                };
                repository.Add(entity);
                await repository.SaveChangesAsync();

                await eventService.Publish(new OnSettlementOperationEvent
                {
                    Id = entity.Id,
                    Operation = EntityOperation.Create,
                });
            }
            if (entity.State == SettlementState.Finished)
                return entity;

            var updatableEntity = new UpdatableSettlement(entity, now);
            await command.UpdateAction.Invoke(updatableEntity);

            if (updatableEntity.HasChanges == false)
                return updatableEntity.Model;

            updatableEntity.Model.ModifiedDate = now;

            await repository.SaveChangesAsync();
            await eventService.Publish(new OnSettlementOperationEvent
            {
                Id = entity.Id,
                Operation = EntityOperation.Update,
            });

            if (entity.State == SettlementState.Finished)
                await eventService.Publish(new OnSettlementFinishedEvent
                {
                    Id = entity.Id,
                });

            return updatableEntity.Model;
        }
    }
}