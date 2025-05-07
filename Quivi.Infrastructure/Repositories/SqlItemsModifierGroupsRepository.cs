using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlItemsModifierGroupsRepository : ARepository<ItemsModifierGroup, GetItemsModifierGroupsCriteria>, IItemsModifierGroupsRepository
    {
        public SqlItemsModifierGroupsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<ItemsModifierGroup> GetFilteredQueryable(GetItemsModifierGroupsCriteria criteria)
        {
            var query = Set.AsQueryable();

            if (criteria.IncludeTranslations)
                query = query.Include(q => q.ItemsModifierGroupTranslations);

            if (criteria.IncludeModifiers)
                query = query.Include(r => r.MenuItemModifiers!);

            if (criteria.IncludeParentMenuItems)
                query = query.Include(r => r.ItemsModifierGroupsAssociation!)
                                .ThenInclude(eg => eg.MenuItem);

            if (criteria.MerchantIds != null)
                query = query.Where(r => criteria.MerchantIds.Contains(r.MerchantId));

            if (criteria.Ids != null)
                query = query.Where(r => criteria.Ids.Contains(r.Id));

            if (criteria.MenuItemIds != null)
                query = query.Where(r => r.ItemsModifierGroupsAssociation!.Any(s => criteria.MenuItemIds.Contains(s.MenuItemId)));

            return query.OrderBy(r => r.CreatedDate);
        }
    }
}
