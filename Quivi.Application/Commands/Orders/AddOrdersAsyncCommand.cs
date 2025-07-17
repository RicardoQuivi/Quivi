using Quivi.Application.Extensions.Pos;
using Quivi.Application.Pos.Items;
using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Commands.Orders
{
    public class AddOrdersAsyncCommand : ICommand<Task<string?>>
    {
        public int MerchantId { get; init; }
        public int EmployeeId { get; init; }
        public OrderOrigin OrdersOrigin { get; init; }
        public required IEnumerable<AddOrder> Orders { get; init; }
    }

    public class AddOrdersAsyncCommandHandler : ICommandHandler<AddOrdersAsyncCommand, Task<string?>>
    {
        const int maxDecimalPlaces = 6;

        private readonly IChannelsRepository channelsRepository;
        private readonly IMenuItemsRepository menuItemsRepository;
        private readonly IOrdersRepository ordersRepository;
        private readonly IOrderSequencesRepository sequencesRepository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;
        private readonly IPosSyncService posSyncService;

        public AddOrdersAsyncCommandHandler(IChannelsRepository channelsRepository,
                                            IMenuItemsRepository menuItemsRepository,
                                            IOrdersRepository ordersRepository,
                                            IDateTimeProvider dateTimeProvider,
                                            IEventService eventService,
                                            IPosSyncService posSyncService,
                                            IOrderSequencesRepository orderSequencesRepository)
        {
            this.channelsRepository = channelsRepository;
            this.menuItemsRepository = menuItemsRepository;
            this.ordersRepository = ordersRepository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
            this.posSyncService = posSyncService;
            this.sequencesRepository = orderSequencesRepository;
        }

        public async Task<string?> Handle(AddOrdersAsyncCommand command)
        {
            var itemsPerChannel = GetItemsPerChannel(command.Orders);
            if (itemsPerChannel.Count == 0)
                return null;

            var channelsQuery = await channelsRepository.GetAsync(new GetChannelsCriteria
            {
                MerchantIds = [command.MerchantId],
                Ids = itemsPerChannel.Keys,
                IncludeChannelProfile = true,
            });

            var (menuItemDictionary, itemsPerChannelWithDefaultPrice) = await GetMenuItemsAndPopulateDefaultPrice(command.MerchantId, itemsPerChannel);
            if (itemsPerChannelWithDefaultPrice.Count == 0)
                return null;

            var now = dateTimeProvider.GetUtcNow();
            var addedOrders = new List<Order>();

            var lastOrderOfMerchantQuery = await sequencesRepository.GetAsync(new GetOrderSequencesCriteria
            {
                MerchantIds = [command.MerchantId],
                PageSize = 1,
            });
            var nextSequence = lastOrderOfMerchantQuery.SingleOrDefault()?.SequenceNumber ?? 1;
            foreach (var channel in channelsQuery)
            {
                bool isTakeAway = channel.ChannelProfile!.Features.HasFlag(ChannelFeature.IsTakeAwayOnly) == true;
                var order = new Order
                {
                    OrderType = isTakeAway ? OrderType.TakeAway : OrderType.OnSite,
                    MerchantId = channel.MerchantId,
                    ChannelId = channel.Id,
                    Channel = channel,
                    EmployeeId = command.EmployeeId,
                    Origin = command.OrdersOrigin,
                    PayLater = true,
                    State = OrderState.Requested,
                    ScheduledTo = null,
                    CreatedDate = now,
                    ModifiedDate = now,
                    OrderSequence = new OrderSequence
                    {
                        SequenceNumber = nextSequence,
                        CreatedDate = now,
                        ModifiedDate = now,
                    },
                };
                nextSequence += 1;
                PopulateOrderItems(menuItemDictionary, itemsPerChannelWithDefaultPrice[channel.Id], order);
                ordersRepository.Add(order);
                addedOrders.Add(order);
            }
            await ordersRepository.SaveChangesAsync();

            List<int> orderIds = [];
            foreach (var order in addedOrders)
            {
                orderIds.Add(order.Id);
                await eventService.Publish(new OnOrderOperationEvent
                {
                    MerchantId = order.MerchantId,
                    ChannelId = order.ChannelId,
                    Id = order.Id,
                    Operation = EntityOperation.Create,
                });
            }

            return await posSyncService.ProcessOrders(orderIds, command.MerchantId, OrderState.Requested, false);
        }

        private IReadOnlyDictionary<int, IEnumerable<AddOrderItem>> GetItemsPerChannel(IEnumerable<AddOrder> orders)
        {
            var result = new Dictionary<int, IEnumerable<AddOrderItem>>();
            foreach (var order in orders)
            {
                var items = ConvertToSessionItems(order.Items);
                if (items.Any() == false)
                    continue;

                result.Add(order.ChannelId, items.SelectMany(s => s.Source));
            }
            return result;
        }

        private async Task<(IReadOnlyDictionary<int, MenuItem>, IReadOnlyDictionary<int, IEnumerable<SessionItem>>)> GetMenuItemsAndPopulateDefaultPrice(int merchantId, IReadOnlyDictionary<int, IEnumerable<AddOrderItem>> itemsPerChannel)
        {
            var items = itemsPerChannel.SelectMany(i => i.Value);
            var menuItemQuery = await menuItemsRepository.GetAsync(new GetMenuItemsCriteria
            {
                MerchantIds = [merchantId],
                Ids = itemsPerChannel.SelectMany(r => r.Value.SelectMany(s => s.Extras.Select(e => e.MenuItemId).Prepend(s.MenuItemId))).Distinct(),
                IsDeleted = null,
                PageSize = null,
            });
            var menuItemDictionary = menuItemQuery.ToDictionary(r => r.Id, r => r);

            var outResult = new Dictionary<int, IEnumerable<SessionItem>>();
            foreach (var entry in itemsPerChannel)
            {
                var itemsInChannel = entry.Value.Select(s => new SessionItem
                {
                    MenuItemId = s.MenuItemId,
                    Quantity = s.Quantity,
                    Discount = s.Discount,
                    Price = s.Price ?? menuItemDictionary[s.MenuItemId].Price,
                    Extras = s.Extras.Select(extra => new BaseSessionItem
                    {
                        MenuItemId = extra.MenuItemId,
                        Quantity = extra.Quantity,
                        Price = extra.Price ?? menuItemDictionary[extra.MenuItemId].Price,
                    }).ToList(),
                }).Compress();

                if (itemsInChannel.Any() == false)
                    continue;

                outResult[entry.Key] = itemsInChannel;
            }
            return (menuItemDictionary, outResult);
        }

        private void PopulateOrderItems(IReadOnlyDictionary<int, MenuItem> menuItemsDictionary, IEnumerable<SessionItem> items, Order order)
        {
            order.OrderMenuItems = items.Select(item =>
            {
                var dbItem = menuItemsDictionary[item.MenuItemId];
                decimal originalPrice = item.Price;
                decimal priceAfterDiscount = Math.Round(PriceHelper.CalculatePriceAfterDiscount(originalPrice, item.Discount), maxDecimalPlaces);

                return new OrderMenuItem
                {
                    MenuItemId = item.MenuItemId,
                    MenuItem = dbItem,
                    Order = order,
                    Name = dbItem.Name,
                    Quantity = item.Quantity,
                    FinalPrice = priceAfterDiscount,
                    OriginalPrice = originalPrice,
                    PriceType = PriceType.Unit,
                    VatRate = dbItem.VatRate,
                    Modifiers = item.Extras.Select(m =>
                    {
                        var extraDbItem = menuItemsDictionary[m.MenuItemId];
                        decimal extraOriginalPrice = m.Price;
                        decimal extraPriceAfterDiscount = Math.Round(PriceHelper.CalculatePriceAfterDiscount(extraOriginalPrice, item.Discount), maxDecimalPlaces);
                        return new OrderMenuItem
                        {
                            MenuItemId = m.MenuItemId,
                            MenuItem = extraDbItem,
                            Order = order,
                            Name = extraDbItem.Name,
                            Quantity = m.Quantity * item.Quantity,
                            FinalPrice = extraPriceAfterDiscount,
                            OriginalPrice = extraOriginalPrice,
                            PriceType = PriceType.Unit,
                            VatRate = extraDbItem.VatRate,
                            Modifiers = [],
                            MenuItemModifierGroupId = null, //This could be populated, but a refactor needs to be performed
                            CreatedDate = order.CreatedDate,
                            ModifiedDate = order.ModifiedDate,
                        };
                    }).ToList(),
                    CreatedDate = order.CreatedDate,
                    ModifiedDate = order.ModifiedDate,
                };
            }).ToList();
        }

        public static IEnumerable<SessionItem<AddOrderItem>> ConvertToSessionItems(IEnumerable<AddOrderItem> items)
        {
            var result = SessionItemComparer<AddOrderItem>.Compress(items, s => new SessionItem
            {
                MenuItemId = s.MenuItemId,
                Price = s.Price ?? -1,
                Quantity = s.Quantity,
                Discount = s.Discount,
                Extras = s.Extras?.Select(e => new BaseSessionItem
                {
                    MenuItemId = e.MenuItemId,
                    Price = e.Price ?? -1,
                    Quantity = e.Quantity,
                }) ?? [],
            }, s => s.Extras.Select(e => new AddOrderItem
            {
                MenuItemId = e.MenuItemId,
                Discount = 0.0m,
                Extras = [],
                Price = s.Price ?? -1,
                Quantity = s.Quantity,
            }) ?? []);
            return result;
        }
    }
}