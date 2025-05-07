using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlItemCategoriesRepository : ARepository<ItemCategory, GetItemCategoriesCriteria>, IItemCategoriesRepository
    {
        public SqlItemCategoriesRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<ItemCategory> GetFilteredQueryable(GetItemCategoriesCriteria criteria)
        {
            IQueryable<ItemCategory> query = Set;

            if (criteria.IncludeMenuItems)
                query = query.Include(x => x.MenuItemCategoryAssociations!).ThenInclude(mic => mic.MenuItem);

            if (criteria.IncludeTranslations)
                query = query.Include(x => x.ItemCategoryTranslations);

            if (criteria.Ids != null)
                query = query.Where(x => criteria.Ids.Contains(x.Id));

            if (criteria.MenuItemIds != null)
                query = query.Where(x => criteria.MenuItemIds.Any(itemId => x.MenuItemCategoryAssociations!.Select(s => s.MenuItemId).Contains(itemId)));

            if (criteria.MerchantIds != null)
                query = query.Where(x => criteria.MerchantIds.Contains(x.MerchantId));

            if (criteria.Name != null)
                query = query.Where(x => x.Name.ToLower() == criteria.Name.ToLower());

            if (criteria.IsDeleted.HasValue)
                query = query.Where(x => x.DeletedDate.HasValue == criteria.IsDeleted.Value);

            if (criteria.WithItems.HasValue)
                query = query.Where(q => q.MenuItemCategoryAssociations!.Any(c => c.MenuItem!.DeletedDate.HasValue == false && c.MenuItem.DeletedDate.HasValue == false) == criteria.WithItems.Value);

            return query.OrderBy(p => p.SortIndex);
        }
    }
}
