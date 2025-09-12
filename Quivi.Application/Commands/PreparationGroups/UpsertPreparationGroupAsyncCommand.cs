using Quivi.Application.Extensions.Pos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.Infrastructure.Abstractions.Events.Data.PreparationGroups;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PreparationGroups
{
    public class UpsertPreparationGroupAsyncCommand : ICommand<Task>
    {
        public int MerchantId { get; set; }
        public required IEnumerable<int> OrderIds { get; set; }
    }

    public class UpsertPreparationGroupAsyncCommandHandler : ICommandHandler<UpsertPreparationGroupAsyncCommand, Task>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IPreparationGroupsRepository repository;
        private readonly IPreparationGroupItemsRepository preparationGroupItemsRepository;
        private readonly IOrdersRepository ordersRepository;
        private readonly IMenuItemsRepository menuItemsRepository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpsertPreparationGroupAsyncCommandHandler(IUnitOfWork unitOfWork,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IEventService eventService)
        {
            this.unitOfWork = unitOfWork;
            this.repository = unitOfWork.PreparationGroups;
            this.preparationGroupItemsRepository = unitOfWork.PreparationGroupItems;
            this.ordersRepository = unitOfWork.Orders;
            this.menuItemsRepository = unitOfWork.MenuItems;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task Handle(UpsertPreparationGroupAsyncCommand command)
        {
            var orderIds = command.OrderIds.ToHashSet();
            if (orderIds.Count == 0)
                return;

            var entities = await ordersRepository.GetAsync(new GetOrdersCriteria
            {
                MerchantIds = [command.MerchantId],
                Ids = orderIds,
                AssociatedWithSession = true,
                AssociatedWithPreparationGroup = false,
                IncludeOrderMenuItems = true,
            });

            if (entities.Any() == false)
                return;

            var ordersBySession = entities.GroupBy(o => o.SessionId!.Value);
            var groupsQuery = await repository.GetAsync(new GetPreparationGroupsCriteria
            {
                MerchantIds = [command.MerchantId],
                SessionIds = ordersBySession.Select(o => o.Key),
                States = [PreparationGroupState.Draft],
                Completed = false,
                IncludePreparationGroupItems = true,
                IncludeOrders = true,
                PageIndex = 0,
                PageSize = ordersBySession.Count(),
            });
            var existentGroups = groupsQuery.ToDictionary(g => g.SessionId, g => g);

            var itemsQuery = await menuItemsRepository.GetAsync(new GetMenuItemsCriteria
            {
                Ids = entities.SelectMany(e => e.OrderMenuItems!).Select(e => e.MenuItemId).Distinct(),
                IsDeleted = null,
                PageIndex = 0,
                PageSize = null,
            });
            var itemsDictionary = itemsQuery.ToDictionary(e => e.Id, e => e);

            var groupsWithChanges = new Dictionary<PreparationGroup, IEnumerable<Order>>();
            var now = dateTimeProvider.GetUtcNow();
            foreach (var ordersGroup in ordersBySession)
            {
                PreparationGroup group = GetOrCreatePreparationGroup(command, existentGroups, ordersGroup, now);
                var ordersWithChanges = AssociateItems(group, ordersGroup, itemsDictionary, now);
                groupsWithChanges.Add(group, ordersWithChanges);
            }
            await unitOfWork.SaveChangesAsync();
            await GenerateEvents(groupsWithChanges, now);
        }

        private PreparationGroup GetOrCreatePreparationGroup(UpsertPreparationGroupAsyncCommand command, IReadOnlyDictionary<int, PreparationGroup> existentGroups, IGrouping<int, Order> orders, DateTime now)
        {
            if (existentGroups.TryGetValue(orders.Key, out var existentGroup) == false)
            {
                var newGroup = new PreparationGroup
                {
                    MerchantId = command.MerchantId,
                    Orders = orders.ToList(),
                    SessionId = orders.Key,
                    PreparationGroupItems = new List<PreparationGroupItem>(),
                    CreatedDate = now,
                    ModifiedDate = now,
                };
                repository.Add(newGroup);
                return newGroup;
            }

            existentGroup.ModifiedDate = now;
            foreach (var order in orders)
            {
                if (order.PreparationGroups == null)
                    order.PreparationGroups = new List<PreparationGroup>();
                order.PreparationGroups.Add(existentGroup);
                existentGroup.Orders!.Add(order);
            }
            return existentGroup;
        }

        private IEnumerable<Order> AssociateItems(PreparationGroup group, IEnumerable<Order> addedOrders, IReadOnlyDictionary<int, MenuItem> itemsDictionary, DateTime now)
        {
            var itemsToAdd = addedOrders.SelectMany(o => o.OrderMenuItems!);
            var childrenDictionary = itemsToAdd.Where(g => g.ParentOrderMenuItemId.HasValue)
                                .GroupBy(g => g.ParentOrderMenuItemId!.Value)
                                .ToDictionary(g => g.Key, g => g.AsEnumerable());
            var compressedSessionItems = itemsToAdd.Where(g => g.ParentOrderMenuItemId.HasValue == false).Select(i => new SessionItem
            {
                MenuItemId = i.MenuItemId,
                Discount = 0, //Discount is irrelevant for preparation
                Price = 0, //Price is irrelevant for preparation
                Quantity = i.Quantity,
                Extras = childrenDictionary.TryGetValue(i.Id, out var extras) ? extras.Select(e => new SessionExtraItem
                {
                    ModifierGroupId = e.MenuItemModifierGroupId ?? throw new Exception($"The extra needs to belong to a {nameof(e.MenuItemModifierGroup)}"),
                    MenuItemId = e.MenuItemId,
                    Price = 0, //Price is irrelevant for preparation
                    Quantity = e.Quantity / i.Quantity,
                }) : Enumerable.Empty<SessionExtraItem>(),
            }).Compress();

            //If from compression no items exist, then it means the orders cancel each other.
            //If that's the case, then we sould mark them as complete and move on
            if (compressedSessionItems.Any() == false)
            {
                foreach (var order in addedOrders)
                    order.State = OrderState.Completed;
                return addedOrders;
            }

            //Else, we should update the values
            IEqualityComparer<SessionItem> comparer = new SessionItemComparer();
            var updateInstructions = compressedSessionItems.ToDictionary(k => k, k => k, comparer);
            foreach (var item in group.PreparationGroupItems!.Where(it => it.ParentPreparationGroupItemId.HasValue == false))
            {
                var sessionItem = new SessionItem
                {
                    MenuItemId = item.MenuItemId,
                    Discount = 0.0m, //Discount is irrelevant for preparation
                    Price = 0.0m, //Price is irrelevant for preparation
                    Quantity = item.RemainingQuantity,
                    Extras = item.Extras?.Select(e => new SessionExtraItem
                    {
                        ModifierGroupId = 0, //ModifierGroupId is irrelevant for preparation
                        MenuItemId = e.MenuItemId,
                        Price = 0, //Price is irrelevant for preparation
                        Quantity = e.RemainingQuantity,
                    }).Where(e => e.Quantity > 0) ?? [],
                };

                if (updateInstructions.TryGetValue(sessionItem, out var update) == false)
                    continue;

                var quantityToAdd = update.Quantity;
                item.OriginalQuantity += (int)quantityToAdd;
                item.RemainingQuantity += (int)quantityToAdd;
                updateInstructions.Remove(update);
            }

            foreach (var item in updateInstructions.Values)
            {
                group.PreparationGroupItems!.Add(new PreparationGroupItem
                {
                    MenuItemId = item.MenuItemId,
                    OriginalQuantity = (int)item.Quantity,
                    RemainingQuantity = (int)item.Quantity,
                    PreparationGroup = group,
                    LocationId = itemsDictionary[item.MenuItemId].LocationId,
                    CreatedDate = now,
                    ModifiedDate = now,
                    Extras = item.Extras.Select(e => new PreparationGroupItem
                    {
                        MenuItemId = e.MenuItemId,
                        OriginalQuantity = (int)e.Quantity,
                        RemainingQuantity = (int)e.Quantity,
                        PreparationGroup = group,
                        LocationId = itemsDictionary[e.MenuItemId].LocationId,
                        CreatedDate = now,
                        ModifiedDate = now,
                    }).ToList(),
                });
            }

            //Remove old items
            foreach (var item in group.PreparationGroupItems!.Where(p => p.OriginalQuantity == 0).ToList())
            {
                preparationGroupItemsRepository.Remove(item);
                foreach (var extra in item.Extras ?? [])
                    preparationGroupItemsRepository.Remove(extra);
            }

            if (group.PreparationGroupItems!.Any())
                return addedOrders;

            var changedOrders = group.Orders!.Where(o => o.State != OrderState.Completed);
            foreach (var order in changedOrders)
                order.State = OrderState.Completed;
            return changedOrders;
        }

        private async Task GenerateEvents(IReadOnlyDictionary<PreparationGroup, IEnumerable<Order>> changesDictionary, DateTime now)
        {
            foreach (var entry in changesDictionary)
            {
                var group = entry.Key;

                await eventService.Publish(new OnPreparationGroupOperationEvent
                {
                    Id = group.Id,
                    MerchantId = group.MerchantId,
                    Operation = group.CreatedDate == now ? EntityOperation.Create : EntityOperation.Update,
                    IsCommited = group.State == PreparationGroupState.Committed,
                });

                foreach (var order in entry.Value)
                {
                    await eventService.Publish(new OnOrderOperationEvent
                    {
                        Id = order.Id,
                        MerchantId = order.MerchantId,
                        ChannelId = order.ChannelId,
                        Operation = EntityOperation.Update,
                    });
                }
            }
        }
    }
}