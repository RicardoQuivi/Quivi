using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.Orders;
using Quivi.Application.Queries.Orders;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.Orders;
using Quivi.Pos.Api.Dtos.Responses.Orders;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireEmployee]
    [Authorize]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public OrdersController(IQueryProcessor queryProcessor,
                                    ICommandProcessor commandProcessor,
                                    IIdConverter idConverter,
                                    IMapper mapper)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<GetOrdersResponse> Get([FromQuery] GetOrdersRequest request)
        {
            request ??= new();
            var result = await queryProcessor.Execute(new GetOrdersAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                SessionIds = request.SessionIds?.Select(idConverter.FromPublicId),
                States = request.States,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetOrdersResponse
            {
                Data = mapper.Map<Dtos.Order>(result),
                Page = result.CurrentPage,
                TotalItems = result.TotalItems,
                TotalPages = result.NumberOfPages,
            };
        }

        [HttpPost]
        public async Task<CreateOrdersResponse> Create([FromBody] CreateOrdersRequest request)
        {
            IEnumerable<AddOrder> ordersToAdd = request.Orders.Select(o => new AddOrder
            {
                ChannelId = idConverter.FromPublicId(o.ChannelId),
                Items = o.Items.Select(i => new AddOrderItem
                {
                    MenuItemId = idConverter.FromPublicId(i.MenuItemId),
                    Discount = i.Discount,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Extras = i.Extras.Select(e => new BaseAddOrderItem
                    {
                        MenuItemId = idConverter.FromPublicId(e.MenuItemId),
                        Price = e.Price,
                        Quantity = e.Quantity,
                    }).ToList(),
                }).ToList(),
            }).ToList();

            //TODO: Validate Insertion

            var jobId = await commandProcessor.Execute(new AddOrdersAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                EmployeeId = User.EmployeeId(idConverter)!.Value,
                OrdersOrigin = Domain.Entities.Pos.OrderOrigin.PoS,
                Orders = ordersToAdd,
            });

            return new CreateOrdersResponse
            {
                Data = jobId,
            };
        }
    }
}
