using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;

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

            if (criteria.AvailableAt != null)
            {
                var allAvailableItemIds = Context.Set<AvailabilityGroup>()
                                            .Where(q => q.AssociatedChannelProfiles!.SelectMany(a => a.ChannelProfile!.Channels!).Select(a => a.Id).Contains(criteria.AvailableAt.ChannelId))
                                            .WithWeeklyAvailabilities(m => m.WeeklyAvailabilities!,
                                                                        m => m.Merchant!.TimeZone!,
                                                                        criteria.AvailableAt.UtcDate)
                                            .SelectMany(s => s.AssociatedMenuItems!.Select(m => m.MenuItemId));

                query = query.Where(x => (x.Stock && allAvailableItemIds.Contains(x.Id)) || x.ShowWhenNotAvailable);

                var allValidItemIds1 = Set.Where(x => x.Stock).SelectMany(m => m.AssociatedAvailabilityGroups!.Select(a => a.AvailabilityGroup!))
                                                                .WithWeeklyAvailabilities(m => m.WeeklyAvailabilities!,
                                                                                    m => m.Merchant!.TimeZone,
                                                                                    criteria.AvailableAt.UtcDate)
                                                                .Select(m => m.Id);

                query = query.Where(item => item.ShowWhenNotAvailable || item.MenuItemModifierGroups!.All(m => m.MenuItemModifierGroup!.MinSelection == 0 || m.MenuItemModifierGroup!.MenuItemModifiers!.Where(i => allValidItemIds1.Contains(i.MenuItemId)).Any()));


                query = query.Where(x => x.Stock || x.ShowWhenNotAvailable)
                                .Where(x => allAvailableItemIds.Contains(x.Id) || x.ShowWhenNotAvailable);

                var allValidItemIds2 = Set.Where(x => x.Stock)
                                            .Where(x => allAvailableItemIds.Contains(x.Id))
                                            .Select(m => m.Id);

                query = query.Where(item => item.ShowWhenNotAvailable || item.MenuItemModifierGroups!.All(m => m.MenuItemModifierGroup!.MinSelection == 0 || m.MenuItemModifierGroup!.MenuItemModifiers!.Where(i => allValidItemIds2.Contains(i.MenuItemId)).Any()));
            }

            if (criteria.IncludeAvailabilities)
                query = query.Include(x => x.AssociatedAvailabilityGroups);

            if (criteria.IncludeMenuItemCategoryAssociations)
                query = query.Include(x => x.MenuItemCategoryAssociations!);

            if (criteria.IncludeMenuItemModifiers)
                query = query.Include(x => x.MenuItemModifiers);

            if (criteria.IncludeModifierGroupsAssociations)
                query = query.Include(x => x.MenuItemModifierGroups!);

            if (criteria.IncludeModifierGroupsAssociationsMenuItemModifierGroupMenuItemModifiers)
                query = query.Include(x => x.MenuItemModifierGroups!).ThenInclude(q => q.MenuItemModifierGroup!).ThenInclude(q => q.MenuItemModifiers!);

            if (criteria.IncludeModifierGroupsAssociationsMenuItemModifierGroupMenuItemModifiersMenuItem)
                query = query.Include(x => x.MenuItemModifierGroups!).ThenInclude(q => q.MenuItemModifierGroup).ThenInclude(q => q.MenuItemModifiers!).ThenInclude(q => q.MenuItem);

            if (criteria.IncludeTranslations)
                query = query.Include(x => x.MenuItemTranslations);


            return query.OrderBy(s => s.SortIndex);
        }
    }
}