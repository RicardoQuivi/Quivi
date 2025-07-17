using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.ItemCategories;
using Quivi.Guests.Api.Dtos.Requests.MenuCategories;
using Quivi.Guests.Api.Dtos.Responses.MenuCategories;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Repositories.Data;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuCategoriesController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public MenuCategoriesController(IIdConverter idConverter, IQueryProcessor queryProcessor, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<GetMenuCategoriesResponse> Get([FromQuery] GetMenuCategoriesRequest request)
        {
            var categoriesQuery = await queryProcessor.Execute(new GetItemCategoriesAsyncQuery
            {
                ChannelIds = [idConverter.FromPublicId(request.ChannelId)],
                AvailableAt = new Availability
                {
                    UtcDate = request.AtDate?.UtcDateTime ?? DateTime.UtcNow,
                    ChannelId = idConverter.FromPublicId(request.ChannelId),
                },
                IncludeTranslations = true,
                HasItems = true,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetMenuCategoriesResponse
            {
                Data = mapper.Map<Dtos.MenuCategory>(categoriesQuery),
                Page = categoriesQuery.CurrentPage,
                TotalItems = categoriesQuery.TotalItems,
                TotalPages = categoriesQuery.NumberOfPages,
            };
        }
    }
}