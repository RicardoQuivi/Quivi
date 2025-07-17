using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlOrderMenuItemsRepository : ARepository<OrderMenuItem, GetOrderMenuItemsCriteria>, IOrderMenuItemsRepository
    {
        public SqlOrderMenuItemsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<OrderMenuItem> GetFilteredQueryable(GetOrderMenuItemsCriteria criteria)
        {
            IQueryable<OrderMenuItem> query = Set;

            if(criteria.IncludeMenuItem)
                query = query.Include(q => q.MenuItem);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.OrderIds != null)
                query = query.Where(q => criteria.OrderIds.Contains(q.OrderId));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.Order!.MerchantId));

            if (criteria.SessionIds != null)
                query = query.Where(q => q.Order!.SessionId.HasValue && criteria.SessionIds.Contains(q.Order!.SessionId.Value));

            return query.OrderBy(o => o.Id);
        }
    }
}
