using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlAvailabilityMenuItemAssociationsRepository : ARepository<AvailabilityMenuItemAssociation, GetAvailabilityMenuItemAssociationsCriteria>, IAvailabilityMenuItemAssociationsRepository
    {
        public SqlAvailabilityMenuItemAssociationsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<AvailabilityMenuItemAssociation> GetFilteredQueryable(GetAvailabilityMenuItemAssociationsCriteria criteria)
        {
            IQueryable<AvailabilityMenuItemAssociation> query = Set;

            if (criteria.MerchantIds != null)
                query = query.Where(x => criteria.MerchantIds.Contains(x.AvailabilityGroup!.MerchantId));

            if (criteria.AvailabilityGroupIds != null)
                query = query.Where(x => criteria.AvailabilityGroupIds.Contains(x.AvailabilityGroupId));

            if (criteria.MenuItemIds != null)
                query = query.Where(x => criteria.MenuItemIds.Contains(x.MenuItemId));

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}