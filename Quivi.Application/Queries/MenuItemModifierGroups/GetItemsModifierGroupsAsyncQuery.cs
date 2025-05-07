using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.ItemsModifierGroups
{
    public class GetItemsModifierGroupsAsyncQuery : APagedAsyncQuery<ItemsModifierGroup>
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? MenuItemIds { get; init; }
        public bool? IsDeleted { get; init; }

        public bool IncludeModifiers { get; init; }
        public bool IncludeParentMenuItems { get; init; }
        public bool IncludeTranslations { get; init; }
    }

    public class GetMenuItemModifierGroupsAsyncQueryHandler : APagedQueryAsyncHandler<GetItemsModifierGroupsAsyncQuery, ItemsModifierGroup>
    {
        private readonly IItemsModifierGroupsRepository repository;

        public GetMenuItemModifierGroupsAsyncQueryHandler(IItemsModifierGroupsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<ItemsModifierGroup>> Handle(GetItemsModifierGroupsAsyncQuery query)
        {
            return repository.GetAsync(new GetItemsModifierGroupsCriteria
            {
                Ids = query.Ids,
                MerchantIds = query.MerchantIds,
                MenuItemIds = query.MenuItemIds,
                IsDeleted = query.IsDeleted,

                IncludeModifiers = query.IncludeModifiers,
                IncludeParentMenuItems = query.IncludeParentMenuItems,
                IncludeTranslations = query.IncludeTranslations,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}