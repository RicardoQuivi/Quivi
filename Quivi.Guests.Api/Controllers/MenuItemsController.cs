using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.MenuItems;
using Quivi.Guests.Api.Dtos.Requests.MenuItems;
using Quivi.Guests.Api.Dtos.Responses.MenuItems;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Repositories.Data;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;
        private readonly IDateTimeProvider dateTimeProvider;

        public MenuItemsController(IIdConverter idConverter, IQueryProcessor queryProcessor, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
            this.dateTimeProvider = dateTimeProvider;
        }

        [HttpGet]
        public async Task<GetMenuItemsResponse> Get([FromQuery] GetMenuItemsRequest request)
        {
            var itemsQuery = await queryProcessor.Execute(new GetMenuItemsAsyncQuery
            {
                ChannelIds = [idConverter.FromPublicId(request.ChannelId)],
                ItemCategoryIds = string.IsNullOrWhiteSpace(request.MenuItemCategoryId) ? null : [idConverter.FromPublicId(request.MenuItemCategoryId)],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                HiddenFromGuestsApp = false,
                AvailableAt = request.IgnoreCalendarAvailability ? null : new AvailabilityAt
                {
                    UtcDate = request.AtDate?.UtcDateTime ?? dateTimeProvider.GetUtcNow(),
                    ChannelId = idConverter.FromPublicId(request.ChannelId),
                },
                IncludeModifierGroupsAssociations = true,
                IncludeModifierGroupsAssociationsMenuItemModifierGroupMenuItemModifiersMenuItem = true,
                IncludeTranslations = true,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });
            return new GetMenuItemsResponse
            {
                Data = mapper.Map<Dtos.MenuItem>(itemsQuery),
                Page = itemsQuery.CurrentPage,
                TotalItems = itemsQuery.TotalItems,
                TotalPages = itemsQuery.NumberOfPages,
            };
        }
    }
}