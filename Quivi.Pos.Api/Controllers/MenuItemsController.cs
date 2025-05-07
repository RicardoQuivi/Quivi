using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.MenuItems;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.MenuItems;
using Quivi.Pos.Api.Dtos.Responses.MenuItems;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireEmployee]
    [Authorize]
    [ApiController]
    public class MenuItemsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public MenuItemsController(IQueryProcessor queryProcessor,
                                    IIdConverter idConverter,
                                    IMapper mapper)
        {
            this.queryProcessor = queryProcessor;
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
    }
}
