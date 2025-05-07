using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPosChargesRepository : ARepository<PosCharge, GetPosChargesCriteria>, IPosChargesRepository
    {
        public SqlPosChargesRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PosCharge> GetFilteredQueryable(GetPosChargesCriteria criteria)
        {
            IQueryable<PosCharge> query = Set;

            if (criteria.IncludePosChargeSelectedMenuItems)
                query = query.Include(q => q.PosChargeSelectedMenuItems);

            if (criteria.IncludePosChargeInvoiceItems)
                query = query.Include(q => q.PosChargeInvoiceItems);

            if (criteria.IncludeMerchant)
                query = query.Include(q => q.Merchant);

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.ChargeId));

            return query.OrderBy(o => o.ChargeId);
        }
    }
}