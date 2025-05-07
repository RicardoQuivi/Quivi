using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.MenuItems
{
    public class GetMenuItemsAsyncQuery : APagedAsyncQuery<MenuItem>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public string? Search { get; init; }
        public IEnumerable<int>? ItemCategoryIds { get; init; }
        public DateTime? AvailableAtUtcDate { get; init; }
        public bool? IsDeleted { get; init; }
        public bool? Stock { get; init; }
        public bool? HasCategory { get; init; }

        public bool IncludeWeeklyAvailabilities { get; init; }
        public bool IncludeMenuItemCategoryAssociations { get; init; }
        public bool IncludeCategories { get; init; }
        public bool IncludeModifierGroups { get; init; }
        public bool IncludeTranslations { get; init; }
        public bool IncludeMenuItemModifiers { get; init; }
    }

    public class GetMenuItemsAsyncQueryHandler : APagedQueryAsyncHandler<GetMenuItemsAsyncQuery, MenuItem>
    {
        private readonly IMenuItemsRepository repository;

        public GetMenuItemsAsyncQueryHandler(IMenuItemsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<MenuItem>> Handle(GetMenuItemsAsyncQuery query)
        {
            return repository.GetAsync(new GetMenuItemsCriteria
            {
                Ids = query.Ids,
                ItemCategoryIds = query.ItemCategoryIds,
                MerchantIds = query.MerchantIds,
                ChannelIds = query.ChannelIds,
                HasCategory = query.HasCategory,
                IsDeleted = query.IsDeleted,
                Search = query.Search,
                Stock = query.Stock,
                AvailableAtUtcDate = query.AvailableAtUtcDate,

                IncludeMenuItemCategoryAssociations = query.IncludeMenuItemCategoryAssociations,
                IncludeCategories = query.IncludeCategories,
                IncludeMenuItemModifiers = query.IncludeMenuItemModifiers,
                IncludeModifierGroups = query.IncludeModifierGroups,
                IncludeTranslations = query.IncludeTranslations,
                IncludeWeeklyAvailabilities = query.IncludeWeeklyAvailabilities,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
