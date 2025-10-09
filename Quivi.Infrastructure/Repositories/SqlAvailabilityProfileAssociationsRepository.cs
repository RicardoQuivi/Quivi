using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlAvailabilityProfileAssociationsRepository : ARepository<AvailabilityProfileAssociation, GetAvailabilityProfileAssociationsCriteria>, IAvailabilityProfileAssociationsRepository
    {
        public SqlAvailabilityProfileAssociationsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<AvailabilityProfileAssociation> GetFilteredQueryable(GetAvailabilityProfileAssociationsCriteria criteria)
        {
            IQueryable<AvailabilityProfileAssociation> query = Set;

            if (criteria.MerchantIds != null)
                query = query.Where(x => criteria.MerchantIds.Contains(x.AvailabilityGroup!.MerchantId));

            if (criteria.AvailabilityGroupIds != null)
                query = query.Where(x => criteria.AvailabilityGroupIds.Contains(x.AvailabilityGroupId));

            if (criteria.ChannelProfileIds != null)
                query = query.Where(x => criteria.ChannelProfileIds.Contains(x.ChannelProfileId));

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}