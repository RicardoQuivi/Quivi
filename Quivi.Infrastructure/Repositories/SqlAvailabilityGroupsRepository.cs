using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlAvailabilityGroupsRepository : ARepository<AvailabilityGroup, GetAvailabilityGroupsCriteria>, IAvailabilityGroupsRepository
    {
        public SqlAvailabilityGroupsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<AvailabilityGroup> GetFilteredQueryable(GetAvailabilityGroupsCriteria criteria)
        {
            IQueryable<AvailabilityGroup> query = Set;

            if (criteria.IncludeWeeklyAvailabilities)
                query = query.Include(q => q.WeeklyAvailabilities);

            if (criteria.IncludeAssociatedChannelProfiles)
                query = query.Include(q => q.AssociatedChannelProfiles);

            if (criteria.IncludeAssociatedMenuItems)
                query = query.Include(q => q.AssociatedMenuItems);

            if (criteria.MerchantIds != null)
                query = query.Where(x => criteria.MerchantIds.Contains(x.MerchantId));

            if (criteria.Ids != null)
                query = query.Where(x => criteria.Ids.Contains(x.Id));

            if (criteria.AutoAddNewChannelProfiles.HasValue)
                query = query.Where(x => x.AutoAddNewChannelProfiles == criteria.AutoAddNewChannelProfiles.Value);

            if (criteria.AutoAddNewMenuItems.HasValue)
                query = query.Where(x => x.AutoAddNewMenuItems == criteria.AutoAddNewMenuItems.Value);

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}