using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.ItemCategories;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.MenuCategories;
using Quivi.Pos.Api.Dtos.Responses.MenuCategories;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireEmployee]
    [RequireSubMerchant]
    [Authorize]
    [ApiController]
    public class MenuCategoriesController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public MenuCategoriesController(IQueryProcessor queryProcessor,
                                    IIdConverter idConverter,
                                    IMapper mapper)
        {
            this.queryProcessor = queryProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<GetMenuCategoriesResponse> Get([FromQuery] GetMenuCategoriesRequest request)
        {
            request = request ?? new GetMenuCategoriesRequest();
            var result = await queryProcessor.Execute(new GetItemCategoriesAsyncQuery
            {
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                HasItems = request.HasItems,
                Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search,
                IsDeleted = false,

                PageSize = request.PageSize,
                PageIndex = request.Page,
            });

            return new GetMenuCategoriesResponse
            {
                Data = mapper.Map<Dtos.MenuCategory>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }
    }
}
