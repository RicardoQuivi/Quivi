using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.ItemCategories
{
    public class GetItemCategoriesAsyncQuery : APagedAsyncQuery<ItemCategory>
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MenuItemIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public string? Search { get; init; }
        public string? Name { get; init; }
        public bool? IsDeleted { get; init; }
        public bool? HasItems { get; set; }
        public Availability? AvailableAt { get; set; }

        public bool IncludeMenuItems { get; set; }
        public bool IncludeTranslations { get; set; }
    }

    public class GetItemCategoriesAsyncQueryHandler : APagedQueryAsyncHandler<GetItemCategoriesAsyncQuery, ItemCategory>
    {
        private readonly IItemCategoriesRepository repository;

        public GetItemCategoriesAsyncQueryHandler(IItemCategoriesRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<ItemCategory>> Handle(GetItemCategoriesAsyncQuery query)
        {
            return repository.GetAsync(new GetItemCategoriesCriteria
            {
                Ids = query.Ids,
                MenuItemIds = query.MenuItemIds,
                ChannelIds = query.ChannelIds,
                MerchantIds = query.MerchantIds,
                Search = query.Search,
                Name = query.Name,
                IsDeleted = query.IsDeleted,
                AvailableAt = query.AvailableAt,
                WithItems = query.HasItems,

                IncludeMenuItems = query.IncludeMenuItems,
                IncludeTranslations = query.IncludeTranslations,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}