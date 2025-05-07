using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PreparationGroups;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using System.Collections;

namespace Quivi.Application.Commands.PreparationGroups
{
    public interface IUpdatablePreparationGroupItem
    {
        int Id { get; }
        int OriginalQuantity { get; }
        int RemainingQuantity { get; set; }
    }

    public interface IUpdatablePreparationGroupItems : IEnumerable<IUpdatablePreparationGroupItem>
    {
        IUpdatablePreparationGroupItem this[int id] { get; }
    }

    public interface IUpdatablePreparationGroup : IUpdatableEntity
    {
        int Id { get; }
        IUpdatablePreparationGroupItems Items { get; }
    }


    public class UpdatePreparationGroupsAsyncCommand : AUpdateAsyncCommand<IEnumerable<PreparationGroup>, IUpdatablePreparationGroup>
    {
        public required GetPreparationGroupsCriteria Criteria { get; init; }
    }

    public class UpdatePreparationGroupAsyncCommandHandler : ICommandHandler<UpdatePreparationGroupsAsyncCommand, Task<IEnumerable<PreparationGroup>>>
    {
        private readonly IPreparationGroupsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdatePreparationGroupAsyncCommandHandler(IPreparationGroupsRepository repository,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        private class UpdatablePreparationGroupItem : IUpdatablePreparationGroupItem
        {
            public readonly PreparationGroupItem Entity;
            private readonly int originalRemainingQuantity;

            public UpdatablePreparationGroupItem(PreparationGroupItem item)
            {
                Entity = item;
                originalRemainingQuantity = Entity.RemainingQuantity;
            }

            public int Id => Entity.Id;
            public int OriginalQuantity => Entity.OriginalQuantity;
            public int RemainingQuantity
            {
                get => Entity.RemainingQuantity;
                set => Entity.RemainingQuantity = value;
            }

            public bool HasChanges
            {
                get => originalRemainingQuantity != Entity.RemainingQuantity;
            }
        }

        private class UpdatablePreparationGroup : IUpdatablePreparationGroup, IUpdatablePreparationGroupItems
        {
            public readonly PreparationGroup Entity;
            public readonly IReadOnlyDictionary<int, UpdatablePreparationGroupItem> ItemsDictionary;

            public UpdatablePreparationGroup(PreparationGroup preparationGroup)
            {
                Entity = preparationGroup;
                ItemsDictionary = preparationGroup.PreparationGroupItems!.ToDictionary(p => p.Id, p => new UpdatablePreparationGroupItem(p));
            }

            public int Id => Entity.Id;

            public IUpdatablePreparationGroupItem this[int id] => ItemsDictionary[id];
            public IUpdatablePreparationGroupItems Items => this;

            public IEnumerator<IUpdatablePreparationGroupItem> GetEnumerator() => ItemsDictionary.Values.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => ItemsDictionary.Values.GetEnumerator();

            public bool HasChanges => ItemsDictionary.Values.Any(p => p.HasChanges);
        }

        public async Task<IEnumerable<PreparationGroup>> Handle(UpdatePreparationGroupsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludePreparationGroupItems = true,
            });
            if (entities.Any() == false)
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatablePreparationGroup> changedEntities = new();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatablePreparationGroup(entity);
                await command.UpdateAction(updatableEntity);

                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);

                    foreach (var item in updatableEntity.ItemsDictionary.Values.Where(p => p.HasChanges))
                        item.Entity.ModifiedDate = now;
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();

            foreach(var updatableEntity in changedEntities)
                await GenerateEvents(updatableEntity);

            return entities;
        }

        private async Task GenerateEvents(UpdatablePreparationGroup updatableEntity)
        {
            var group = updatableEntity.Entity;
            await eventService.Publish(new OnPreparationGroupOperationEvent
            {
                MerchantId = group.MerchantId,
                Id = group.Id,
                IsCommited = group.State == PreparationGroupState.Committed,
                Operation = EntityOperation.Update,
            });

            if (group.PreparationGroupItems!.Any(i => i.RemainingQuantity != 0))
                return;

            await eventService.Publish(new OnPreparationGroupCompletedEvent
            {
                MerchantId = group.MerchantId,
                Id = group.Id,
                ParentPreparationGroupId = group.ParentPreparationGroupId,
            });
        }
    }
}