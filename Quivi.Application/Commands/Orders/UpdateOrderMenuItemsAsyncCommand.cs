using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Orders
{
    public class UpdateOrderMenuItem : BaseUpdateOrderMenuItem
    {
        public required IEnumerable<UpdateOrderMenuItemModifier> Modifiers { get; init; }
    }

    public class UpdateOrderMenuItemModifier : BaseUpdateOrderMenuItem
    {
        public int ItemsModifierGroupId { get; init; }
    }

    public abstract class BaseUpdateOrderMenuItem
    {
        public required string Name { get; init; }
        public decimal FinalPrice { get; init; }
        public PriceType PriceType { get; init; }
        public decimal VatRate { get; init; }
        public int MenuItemId { get; init; }
        public decimal Quantity { get; init; }
    }

    //TODO: This should be refactored to use IUpdatableEntities instead
    public class UpdateOrderMenuItemsAsyncCommand : ICommand<Task<Order>>
    {
        public int OrderId { get; init; }
        public required IEnumerable<UpdateOrderMenuItem> MenuItems { get; init; }
    }

    public class UpdateOrderMenuItemsAsyncCommandHandler : ICommandHandler<UpdateOrderMenuItemsAsyncCommand, Task<Order>>
    {
        private readonly IOrdersRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateOrderMenuItemsAsyncCommandHandler(IOrderMenuItemsRepository repository,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IEventService eventService, 
                                                        IOrdersRepository ordersRepository)
        {
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
            this.repository = ordersRepository;
        }

        public async Task<Order> Handle(UpdateOrderMenuItemsAsyncCommand command)
        {
            var orderQuery = await repository.GetAsync(new GetOrdersCriteria
            {
                Ids = [command.OrderId],
                IncludeOrderMenuItems = true,
                PageSize = null,
            });
            var order = orderQuery.Single();
            if(order.State != OrderState.Draft)
                throw new Exception("Cannot update order items for an order that is not in draft state.");

            var now = dateTimeProvider.GetUtcNow();
            foreach(var omi in order.OrderMenuItems!)
                omi.Quantity = 0;

            foreach (var item in command.MenuItems)
            {
                var orderMenuItem = new OrderMenuItem
                {
                    MenuItemId = item.MenuItemId,
                    MenuItem = null,
                    Order = null,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    FinalPrice = item.FinalPrice,
                    OriginalPrice = item.FinalPrice,
                    VatRate = item.VatRate,
                    PriceType = item.PriceType,
                    OrderId = command.OrderId,
                    Modifiers = item.Modifiers?.Select(m => new OrderMenuItem
                    {
                        MenuItemId = m.MenuItemId,
                        Name = m.Name,
                        FinalPrice = m.FinalPrice,
                        OriginalPrice = m.FinalPrice,
                        PriceType = m.PriceType,
                        VatRate = m.VatRate,
                        OrderId = command.OrderId,
                        Quantity = m.Quantity,
                        MenuItemModifierGroupId = m.ItemsModifierGroupId,
                        CreatedDate = now,
                    }).ToList(),
                    CreatedDate = now,
                    ModifiedDate = now,
                };
                order.OrderMenuItems.Add(orderMenuItem);
            }
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnOrderOperationEvent
            {
                Id = order.Id,
                ChannelId = order.ChannelId,
                MerchantId = order.MerchantId,
                Operation = EntityOperation.Update,
            });
            return order;
        }
    }
}
