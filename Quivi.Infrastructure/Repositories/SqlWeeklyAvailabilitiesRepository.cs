using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Domain.Repositories.EntityFramework.Functions;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlWeeklyAvailabilitiesRepository : ARepository<WeeklyAvailability, GetWeeklyAvailabilitiesCriteria>, IWeeklyAvailabilitiesRepository
    {
        public SqlWeeklyAvailabilitiesRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<WeeklyAvailability> GetFilteredQueryable(GetWeeklyAvailabilitiesCriteria criteria)
        {
            IQueryable<WeeklyAvailability> query = Set;

            if (criteria.IncludeAvailabilityGroup)
                query = query.Include(e => e.AvailabilityGroup);

            if (criteria.IncludeAvailabilityGroupAssociatedMenuItems)
                query = query.Include(e => e.AvailabilityGroup!).ThenInclude(q => q.AssociatedMenuItems);

            if (criteria.IncludeAvailabilityGroupAssociatedChannelProfiles)
                query = query.Include(e => e.AvailabilityGroup!).ThenInclude(q => q.AssociatedChannelProfiles);

            if (criteria.MerchantIds != null)
                query = query.Where(x => criteria.MerchantIds.Contains(x.AvailabilityGroup!.MerchantId));

            if (criteria.ChannelProfileIds != null)
                query = query.Where(x => x.AvailabilityGroup!.AssociatedChannelProfiles!.Any(a => criteria.ChannelProfileIds.Contains(a.ChannelProfileId)));

            if (criteria.MenuItemIds != null)
                query = query.Where(x => x.AvailabilityGroup!.AssociatedMenuItems!.Any(a => criteria.MenuItemIds.Contains(a.MenuItemId)));

            if (criteria.ChangesOnDate != null)
                query = query.Where(e => criteria.ChangesOnDate.Any(d => e.StartAtSeconds == QuiviDbFunctions.ToWeeklyAvailabilityInSeconds(QuiviDbFunctions.ToTimeZone(d, e.AvailabilityGroup!.Merchant!.TimeZone)) ||
                                                                        e.EndAtSeconds == QuiviDbFunctions.ToWeeklyAvailabilityInSeconds(QuiviDbFunctions.ToTimeZone(d, e.AvailabilityGroup.Merchant.TimeZone))));

            return query.OrderBy(q => q.Id);
        }
    }
}