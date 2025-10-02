using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PreparationGroups;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Jobs.Hangfire.Context;
using Quivi.Infrastructure.Jobs.Hangfire.Filters;

namespace Quivi.Application.Commands.PreparationGroups
{
    public class AddCommitedPreparationGroupAsyncCommand : ICommand<Task<string>>
    {
        public int MerchantId { get; init; }
        public int PreparationGroupId { get; init; }
        public string? Note { get; init; }
        public bool IsPrepared { get; init; }
        public IReadOnlyDictionary<int, int>? PreparationGroupItemsQuantities { get; init; }
        public int? SourceLocationId { get; init; }
    }

    public class AddCommitedPreparationGroupAsyncCommandHandler : ICommandHandler<AddCommitedPreparationGroupAsyncCommand, Task<string>>
    {
        protected readonly IPreparationGroupsRepository repository;
        protected readonly IDateTimeProvider dateTimeProvider;
        protected readonly IBackgroundJobHandler backgroundJobHandler;
        protected readonly IPosSyncService posSyncService;
        protected readonly IEventService eventService;

        public AddCommitedPreparationGroupAsyncCommandHandler(IPreparationGroupsRepository repository,
                                                                IDateTimeProvider dateTimeProvider,
                                                                IBackgroundJobHandler backgroundJobHandler,
                                                                IPosSyncService posSyncService,
                                                                IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.backgroundJobHandler = backgroundJobHandler;
            this.posSyncService = posSyncService;
            this.eventService = eventService;
        }

        public Task<string> Handle(AddCommitedPreparationGroupAsyncCommand command)
        {
            var jobId = backgroundJobHandler.Enqueue(() => Execute(command));
            return Task.FromResult(jobId);
        }

        public void Contextualize(IJobContextualizer contextualizer, AddCommitedPreparationGroupAsyncCommand command)
        {
            contextualizer.MerchantId = command.MerchantId;
        }

        [ContextualizeFilter(nameof(Contextualize))]
        [PerMerchantDistributedLockFilter]
        public virtual async Task Execute(AddCommitedPreparationGroupAsyncCommand command)
        {
            var preparationGroupQuery = await repository.GetAsync(new GetPreparationGroupsCriteria
            {
                MerchantIds = [command.MerchantId],
                Ids = [command.PreparationGroupId],
                States = [PreparationGroupState.Draft],
                Completed = false,
                IncludePreparationGroupItems = true,
                IncludeOrders = true,
            });
            var preparationGroup = preparationGroupQuery.SingleOrDefault();
            if (preparationGroup == null)
                return;

            var now = dateTimeProvider.GetUtcNow();
            var entity = GetEntity(preparationGroup, command, now);
            if (entity == null)
                return;

            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnPreparationGroupOperationEvent
            {
                MerchantId = entity.MerchantId,
                Id = entity.Id,
                IsCommited = true,
                Operation = EntityOperation.Create,
            });
            await eventService.Publish(new OnPreparationGroupOperationEvent
            {
                MerchantId = entity.MerchantId,
                Id = preparationGroup.Id,
                IsCommited = true,
                Operation = EntityOperation.Update,
            });

            if (entity.PreparationGroupItems!.Any(i => i.RemainingQuantity > 0))
            {
                backgroundJobHandler.Enqueue<ICommandProcessor>(c => c.Execute(new PrintPreparationGroupAsyncCommand
                {
                    MerchantId = entity.MerchantId,
                    PreparationGroupId = entity.Id,
                    LocationId = command.SourceLocationId,
                }));
                return;
            }

            await eventService.Publish(new OnPreparationGroupCompletedEvent
            {
                MerchantId = entity.MerchantId,
                Id = preparationGroup.Id,
                ParentPreparationGroupId = preparationGroup.ParentPreparationGroupId,
            });
        }

        private PreparationGroup? GetEntity(PreparationGroup preparationGroup, AddCommitedPreparationGroupAsyncCommand command, DateTime now)
        {
            var items = GetItems(preparationGroup, command.PreparationGroupItemsQuantities, now, command.IsPrepared, command.SourceLocationId);
            if (items.Count == 0)
                return null;

            var entity = new PreparationGroup
            {
                ParentPreparationGroup = preparationGroup,
                ParentPreparationGroupId = preparationGroup.Id,

                State = PreparationGroupState.Committed,
                AdditionalNote = command.Note,
                PreparationGroupItems = items,
                CreatedDate = now,
                ModifiedDate = now,

                Orders = preparationGroup.Orders!.ToList(),

                Session = preparationGroup.Session,
                SessionId = preparationGroup.SessionId,

                Merchant = preparationGroup.Merchant,
                MerchantId = preparationGroup.MerchantId,
            };

            return entity;
        }

        private ICollection<PreparationGroupItem> GetItems(PreparationGroup preparationGroup, IReadOnlyDictionary<int, int>? preparationGroupItemsQuantities, DateTime now, bool isPrepared, int? locationId)
        {
            IDictionary<int, PreparationGroupItem> result = new Dictionary<int, PreparationGroupItem>();
            foreach (var item in preparationGroup.PreparationGroupItems!.OrderBy(p => p.ParentPreparationGroupItemId ?? 0))
            {
                var totalQuantity = item.OriginalQuantity;
                var availableQuantityToCommited = item.RemainingQuantity;

                if (availableQuantityToCommited == 0)
                    continue;

                var quantity = availableQuantityToCommited;
                if (preparationGroupItemsQuantities != null && preparationGroupItemsQuantities.TryGetValue(item.Id, out quantity) == false)
                    continue;

                PreparationGroupItem? parent = null;
                if (item.ParentPreparationGroupItemId.HasValue && result.TryGetValue(item.ParentPreparationGroupItemId.Value, out parent) == false)
                {
                    parent = new PreparationGroupItem
                    {
                        OriginalQuantity = 0,
                        RemainingQuantity = 0,

                        ParentPreparationGroupItem = null,
                        ParentPreparationGroupItemId = null,

                        MenuItemId = item.ParentPreparationGroupItem!.MenuItemId,
                        MenuItem = item.ParentPreparationGroupItem!.MenuItem,

                        LocationId = item.ParentPreparationGroupItem.LocationId ?? locationId,

                        CreatedDate = now,
                        ModifiedDate = now,
                    };
                    result.Add(item.ParentPreparationGroupItemId.Value, parent);
                }

                item.RemainingQuantity -= quantity;
                result.Add(item.Id, new PreparationGroupItem
                {
                    OriginalQuantity = quantity,
                    RemainingQuantity = isPrepared ? 0 : quantity,

                    ParentPreparationGroupItem = parent,

                    MenuItemId = item.MenuItemId,
                    MenuItem = item.MenuItem,

                    LocationId = item.LocationId ?? locationId,

                    CreatedDate = now,
                    ModifiedDate = now,
                });
            }
            return result.Values;
        }
    }
}