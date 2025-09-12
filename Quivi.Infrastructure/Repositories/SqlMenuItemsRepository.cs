using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlMenuItemsRepository : ARepository<MenuItem, GetMenuItemsCriteria>, IMenuItemsRepository
    {
        public SqlMenuItemsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<MenuItem> GetFilteredQueryable(GetMenuItemsCriteria criteria)
        {
            IQueryable<MenuItem> query = Set;

            if (criteria.MerchantIds != null)
                query = query.Where(x => criteria.MerchantIds.Contains(x.MerchantId));

            if (criteria.Ids != null)
                query = query.Where(x => criteria.Ids.Contains(x.Id));

            if (!string.IsNullOrWhiteSpace(criteria.Search))
                query = query.Where(x => EF.Functions.Collate(x.Name, "Latin1_General_CI_AI").Contains(criteria.Search));

            if (criteria.ItemCategoryIds != null)
                query = query.Where(x => x.MenuItemCategoryAssociations!.Any(mic => !mic.ItemCategory!.DeletedDate.HasValue && criteria.ItemCategoryIds.Contains(mic.ItemCategoryId)));

            if (criteria.HasCategory.HasValue)
                query = query.Where(x => x.MenuItemCategoryAssociations!.Any(mic => !mic.ItemCategory!.DeletedDate.HasValue) == criteria.HasCategory.Value);

            if (criteria.IsDeleted.HasValue)
                query = query.Where(x => x.DeletedDate.HasValue == criteria.IsDeleted.Value);

            if (criteria.Stock.HasValue)
                query = query.Where(x => x.Stock == criteria.Stock.Value);

            if (criteria.HiddenFromGuestsApp.HasValue)
                query = query.Where(q => q.HiddenFromGuestApp == criteria.HiddenFromGuestsApp.Value);

            if (criteria.IncludeWeeklyAvailabilities)
                query = query.Include(x => x.MenuItemWeeklyAvailabilities);

            if (criteria.IncludeMenuItemCategoryAssociations)
                query = query.Include(x => x.MenuItemCategoryAssociations!);

            if (criteria.IncludeMenuItemModifiers)
                query = query.Include(x => x.MenuItemModifiers);

            if (criteria.IncludeModifierGroupsAssociations)
            {
                query = query.Include(x => x.MenuItemModifierGroups!);
                //.ThenInclude(mg => mg.MenuItemModifierGroup!)
                //.ThenInclude(x => x.MenuItemModifiers!)
                //.ThenInclude(m => m.MenuItem!);

                //if (criteria.IncludeTranslations)
                //{
                //    query = query.Include(g => g.MenuItemModifierGroups!)
                //                    .ThenInclude(g => g.MenuItemModifierGroup)
                //                    .ThenInclude(g => g.ItemsModifierGroupTranslations);

                //    query = query.Include(x => x.MenuItemModifierGroups!)
                //                    .ThenInclude(mg => mg.MenuItemModifierGroup!)
                //                    .ThenInclude(mg => mg.MenuItemModifiers!)
                //                    .ThenInclude(m => m.MenuItem!)
                //                    .ThenInclude(m => m.MenuItemTranslations!);
                //}

                //if (criteria.IncludeWeeklyAvailabilities)
                //    query = query.Include(x => x.MenuItemModifierGroups!)
                //                    .ThenInclude(mg => mg.MenuItemModifierGroup!)
                //                    .ThenInclude(mg => mg.MenuItemModifiers!)
                //                    .ThenInclude(m => m.MenuItem!)
                //                    .ThenInclude(m => m.MenuItemWeeklyAvailabilities!);
            }

            if (criteria.IncludeModifierGroupsAssociationsMenuItemModifierGroupMenuItemModifiers)
                query = query.Include(x => x.MenuItemModifierGroups!).ThenInclude(q => q.MenuItemModifierGroup).ThenInclude(q => q.MenuItemModifiers!);

            if (criteria.IncludeModifierGroupsAssociationsMenuItemModifierGroupMenuItemModifiersMenuItem)
                query = query.Include(x => x.MenuItemModifierGroups!).ThenInclude(q => q.MenuItemModifierGroup).ThenInclude(q => q.MenuItemModifiers!).ThenInclude(q => q.MenuItem);

            if (criteria.IncludeTranslations)
                query = query.Include(x => x.MenuItemTranslations);


            return query.OrderBy(s => s.SortIndex);
        }
    }
}
