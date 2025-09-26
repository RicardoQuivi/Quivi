using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.MenuItems;
using Quivi.Application.Queries.MenuItems;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.MenuItems;
using Quivi.Pos.Api.Dtos.Responses.MenuItems;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireEmployee]
    [RequireSubMerchant]
    [Authorize]
    [ApiController]
    public class MenuItemsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public MenuItemsController(IQueryProcessor queryProcessor,
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
        public async Task<GetMenuItemsResponse> Get([FromQuery] GetMenuItemsRequest request)
        {
            request = request ?? new GetMenuItemsRequest();
            var result = await queryProcessor.Execute(new GetMenuItemsAsyncQuery
            {
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                ItemCategoryIds = string.IsNullOrWhiteSpace(request.MenuCategoryId) ? null : [idConverter.FromPublicId(request.MenuCategoryId)],
                Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search,
                IsDeleted = request.IncludeDeleted ? null : false,

                IncludeModifierGroupsAssociationsMenuItemModifierGroupMenuItemModifiers = true,
                IncludeMenuItemCategoryAssociations = true,

                PageSize = request.PageSize,
                PageIndex = request.Page,
            });

            return new GetMenuItemsResponse
            {
                Data = mapper.Map<Dtos.MenuItem>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPatch]
        public async Task<UpdateMenuItemStockResponse> UpdateStock([FromBody] UpdateMenuItemStockRequest request)
        {
            var stockMap = request.StockMap.ToDictionary(kv => idConverter.FromPublicId(kv.Key), kv => kv.Value);
            var result = await commandProcessor.Execute(new UpdateMenuItemAsyncCommand
            {
                Criteria = new GetMenuItemsCriteria
                {
                    Ids = stockMap.Keys,
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],


                    IncludeModifierGroupsAssociationsMenuItemModifierGroupMenuItemModifiers = true,
                },
                UpdateAction = item =>
                {
                    item.HasStock = stockMap[item.Id];
                    return Task.CompletedTask;
                },
            });
            return new UpdateMenuItemStockResponse
            {
                Data = mapper.Map<Dtos.MenuItem>(result),
            };
        }
    }
}
