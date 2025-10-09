using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Commands.Orders;
using Quivi.Application.Queries.MenuItems;
using Quivi.Application.Queries.OrderConfigurableFields;
using Quivi.Application.Queries.Orders;
using Quivi.Domain.Entities.Pos;
using Quivi.Guests.Api.Dtos.Requests.Orders;
using Quivi.Guests.Api.Dtos.Responses.Orders;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Repositories.Data;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IIdConverter idConverter;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IPosSyncService posSyncService;
        private readonly IMapper mapper;

        public OrdersController(IIdConverter idConverter,
                                IQueryProcessor queryProcessor,
                                IMapper mapper,
                                ICommandProcessor commandProcessor,
                                IDateTimeProvider dateTimeProvider,
                                IPosSyncService posSyncService)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
            this.commandProcessor = commandProcessor;
            this.dateTimeProvider = dateTimeProvider;
            this.posSyncService = posSyncService;
        }


        [HttpGet]
        public async Task<GetOrdersResponse> Get([FromQuery] GetOrdersRequest request)
        {
            if (new[] { request.Ids, request.ChargeIds, request.ChannelIds }.All(c => c == null) && string.IsNullOrWhiteSpace(request.SessionId))
                return new GetOrdersResponse
                {
                    Data = [],
                    Page = 0,
                    TotalItems = 0,
                    TotalPages = 0,
                };

            IEnumerable<OrderState>? states = request.Ids != null || request.ChargeIds != null
                            ?
                                null
                            :
                            (
                                string.IsNullOrWhiteSpace(request.SessionId)
                                ?
                                [OrderState.PendingApproval]
                                :
                                [OrderState.PendingApproval, OrderState.Accepted, OrderState.Scheduled, OrderState.Processing, OrderState.Rejected, OrderState.Completed]
                            );

            var ordersQuery = await queryProcessor.Execute(new GetOrdersAsyncQuery
            {
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                ChargeIds = request.ChargeIds?.Select(idConverter.FromPublicId),
                ChannelIds = request.ChannelIds?.Select(idConverter.FromPublicId),
                SessionIds = string.IsNullOrWhiteSpace(request.SessionId) ? null : new[] { idConverter.FromPublicId(request.SessionId) },
                States = states,
                Origins = [OrderOrigin.GuestsApp],

                IncludeOrderMenuItems = true,
                IncludeChangeLogs = true,
                IncludeOrderAdditionalFields = true,
                IncludeOrderMenuItemsPosChargeInvoiceItemsPosCharge = true,
                IncludeOrderSequence = true,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetOrdersResponse
            {
                Data = mapper.Map<Dtos.Order>(ordersQuery),
                Page = ordersQuery.CurrentPage,
                TotalPages = ordersQuery.NumberOfPages,
                TotalItems = ordersQuery.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request)
        {
            var order = await commandProcessor.Execute(new AddOrderAsyncCommand
            {
                ChannelId = idConverter.FromPublicId(request.ChannelId),
            });
            order = await UpdateOrderItems(request.Items, order);
            return new CreateOrderResponse
            {
                Data = mapper.Map<Dtos.Order>(order),
            };
        }

        [HttpPut]
        public async Task<UpdateOrderResponse> UpdateOrder(UpdateOrderRequest request)
        {
            var orders = await commandProcessor.Execute(new UpdateOrdersAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetOrdersCriteria
                {
                    Ids = [idConverter.FromPublicId(request.Id)],
                    IncludeOrderMenuItemsPosChargeInvoiceItemsPosCharge = true,
                    PageSize = 1,
                },
                UpdateAction = async (IUpdatableOrder o) =>
                {
                    foreach (var field in request.Fields ?? new Dictionary<string, string>())
                    {
                        var id = idConverter.FromPublicId(field.Key);
                        o.Fields.Upsert(id, t => t.Value = field.Value);
                    }

                    var configurableFields = await queryProcessor.Execute(new GetOrderConfigurableFieldsAsyncQuery
                    {
                        ChannelIds = [o.ChannelId],
                        ForOrdering = true,
                        IsAutoFill = true,
                        IsDeleted = false,
                        PageSize = null,
                    });

                    foreach (var field in configurableFields)
                        o.Fields.Upsert(field.Id, t => t.Value = field.DefaultValue ?? string.Empty);
                },
            });
            var order = orders.FirstOrDefault();
            if (order == null)
                return new UpdateOrderResponse
                {
                    Data = null,
                };

            order = await UpdateOrderItems(request.Items, order);
            return new UpdateOrderResponse
            {
                Data = mapper.Map<Dtos.Order>(order),
            };
        }

        [HttpPost("{orderId}/submit")]
        public async Task<SubmitOrderResponse> Submit(string orderId)
        {
            var orders = await commandProcessor.Execute(new UpdateOrdersAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetOrdersCriteria
                {
                    Ids = [idConverter.FromPublicId(orderId)],
                    IncludeOrderMenuItemsPosChargeInvoiceItemsPosCharge = true,
                    PageSize = 1,
                },
                UpdateAction = (IUpdatableOrder o) =>
                {
                    o.PayLater = true;
                    return Task.CompletedTask;
                }
            });

            var order = orders.Single();
            var orderIds = orders.Select(s => s.Id).ToList();
            string? jobId = null;
            if (order.State == OrderState.Draft)
                jobId = await posSyncService.ProcessOrders(orderIds, order.MerchantId, OrderState.Draft, false);

            return new SubmitOrderResponse
            {
                Data = mapper.Map<Dtos.Order>(order),
                JobId = jobId!,
            };
        }

        private async Task<Order> UpdateOrderItems(IEnumerable<OrderItem> items, Order order)
        {
            var decodedItems = items.Select(x => new
            {
                Id = idConverter.FromPublicId(x.MenuItemId),
                x.Quantity,
                ModifierGroups = x.ModifierGroups?.Select(g => new
                {
                    Id = idConverter.FromPublicId(g.ModifierId),
                    SelectedOptions = g.SelectedOptions.Select(o => new
                    {
                        Id = idConverter.FromPublicId(o.MenuItemId),
                        o.Quantity,
                    }),
                }) ?? [],
            }).ToList();

            var itemsQuery = await queryProcessor.Execute(new GetMenuItemsAsyncQuery
            {
                MerchantIds = [order.MerchantId],
                Ids = decodedItems.Select(d => d.Id),
                IncludeModifierGroupsAssociations = true,
                IncludeModifierGroupsAssociationsMenuItemModifierGroupMenuItemModifiersMenuItem = true,
                AvailableAt = new AvailabilityAt
                {
                    UtcDate = dateTimeProvider.GetUtcNow(),
                    ChannelId = order.ChannelId,
                },
                PageIndex = 0,
                PageSize = null,
            });

            var modifiersDictionary = decodedItems.GroupBy(d => d.Id).ToDictionary(d => d.Key, d1 => d1.Select(d => d.ModifierGroups.ToDictionary(g => g.Id, g => g.SelectedOptions.Select(s => (s.Id, s.Quantity)))));
            ValidateModifiers(modifiersDictionary, itemsQuery);

            var quantityItems = itemsQuery.Join(decodedItems, x => x.Id, x => x.Id, (dbItem, reqItem) => (DbItem: dbItem, RequestedItem: reqItem));
            return await commandProcessor.Execute(new UpdateOrderMenuItemsAsyncCommand
            {
                OrderId = order.Id,
                MenuItems = quantityItems.Select(x =>
                {
                    var groupsDictionary = x.DbItem.MenuItemModifierGroups!.ToDictionary(s => s.MenuItemModifierGroupId, s => s.MenuItemModifierGroup!.MenuItemModifiers!.ToDictionary(m => m.MenuItemId, m => m));
                    return new UpdateOrderMenuItem
                    {
                        MenuItemId = x.DbItem.Id,
                        FinalPrice = x.DbItem.Price,
                        PriceType = x.DbItem.PriceType,
                        Name = x.DbItem.Name,
                        VatRate = x.DbItem.VatRate,
                        Quantity = x.RequestedItem.Quantity,
                        Modifiers = x.RequestedItem.ModifierGroups.SelectMany(m => m.SelectedOptions.Select(s => new UpdateOrderMenuItemModifier
                        {
                            MenuItemId = s.Id,
                            Quantity = s.Quantity * x.RequestedItem.Quantity,
                            ItemsModifierGroupId = m.Id,

                            FinalPrice = groupsDictionary[m.Id][s.Id].Price,
                            PriceType = groupsDictionary[m.Id][s.Id].MenuItem!.PriceType,
                            Name = groupsDictionary[m.Id][s.Id].MenuItem!.Name,
                            VatRate = groupsDictionary[m.Id][s.Id].MenuItem!.VatRate,
                        })),
                    };
                }),
            });
        }

        private void ValidateModifiers(Dictionary<int, IEnumerable<Dictionary<int, IEnumerable<(int Id, int Quantity)>>>> modifiersDictionary, IEnumerable<MenuItem> items)
        {
            foreach (var item in items)
            {
                if (item.MenuItemModifierGroups!.Any() != true)
                    continue;

                if (modifiersDictionary.TryGetValue(item.Id, out var requestedItemModifiersPerItem) == false)
                    throw new Exception();

                foreach (var g in item.MenuItemModifierGroups!)
                    foreach (var requestedItemModifiers in requestedItemModifiersPerItem)
                    {
                        requestedItemModifiers.TryGetValue(g.MenuItemModifierGroupId, out var requestedSelectedModifiers);

                        var selectedList = requestedSelectedModifiers?.ToDictionary(s => s.Id, s => s.Quantity) ?? [];
                        var totalItemsSelected = selectedList.Values.Sum(s => s);
                        if (totalItemsSelected < g.MenuItemModifierGroup!.MinSelection)
                            throw new Exception();

                        if (totalItemsSelected > g.MenuItemModifierGroup!.MaxSelection)
                            throw new Exception();

                        var dbModifiers = g.MenuItemModifierGroup.MenuItemModifiers!.ToDictionary(m => m.MenuItemId, m => m);
                        foreach (var selectedModifier in selectedList)
                        {
                            if (dbModifiers.ContainsKey(selectedModifier.Key) == false)
                                throw new Exception();
                        }
                    }
            }
        }
    }
}