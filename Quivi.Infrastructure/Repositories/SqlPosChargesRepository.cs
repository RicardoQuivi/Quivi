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

            if (criteria.IncludeCharge)
                query = query.Include(q => q.Charge);

            if (criteria.IncludePosChargeSyncAttempts)
                query = query.Include(q => q.PosChargeSyncAttempts);

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.ChargeId));

            if (criteria.SessionIds != null)
                query = query.Where(q => q.SessionId.HasValue && criteria.SessionIds.Contains(q.SessionId.Value));

            if (criteria.ChannelIds != null)
                query = query.Where(q => criteria.ChannelIds.Contains(q.ChannelId));

            if (criteria.IsCaptured.HasValue)
                query = query.Where(q => q.CaptureDate.HasValue == criteria.IsCaptured.Value);

            if(criteria.OrderIds != null)
            {
                var sessionIds = Context.Orders.Where(o => criteria.OrderIds.Contains(o.Id))
                                                .Where(o => o.SessionId.HasValue)
                                                .Select(o => o.SessionId!.Value)
                                                .Distinct();

                query = query.Where(q => (q.SessionId.HasValue && sessionIds.Contains(q.SessionId.Value)) ||
                                            q.PosChargeSelectedMenuItems!.Select(q => q.OrderMenuItem!).All(omi => criteria.OrderIds.Contains(omi.OrderId)));
            }

            if(criteria.HasSession.HasValue)
                query = query.Where(q => q.SessionId.HasValue == criteria.HasSession.Value);

            return query.OrderBy(o => o.ChargeId);
        }
    }
}