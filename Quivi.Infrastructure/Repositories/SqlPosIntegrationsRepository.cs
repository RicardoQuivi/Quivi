using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPosIntegrationsRepository : ARepository<PosIntegration, GetPosIntegrationsCriteria>, IPosIntegrationsRepository
    {
        public SqlPosIntegrationsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PosIntegration> GetFilteredQueryable(GetPosIntegrationsCriteria criteria)
        {
            IQueryable<PosIntegration> query = Set;

            if (criteria.IncludeMerchant)
                query = query.Include(q => q.Merchant);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.ChannelIds != null)
            {
                var posIntegrationIds = Context.Channels.Where(c => criteria.ChannelIds.Contains(c.Id))
                                                            .Select(c => c.ChannelProfile!.PosIntegrationId);
                query = query.Where(q => posIntegrationIds.Contains(q.Id));
            }

            if (criteria.OrderIds != null)
            {
                var posIntegrationIds = Context.Orders.Where(c => criteria.OrderIds.Contains(c.Id))
                                                            .Select(c => c.Channel!.ChannelProfile!.PosIntegrationId);
                query = query.Where(q => posIntegrationIds.Contains(q.Id));
            }

            if (criteria.ParentMerchantIds != null)
                query = query.Where(q => q.Merchant!.ParentMerchantId.HasValue && criteria.ParentMerchantIds.Contains(q.Merchant.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.ChargeIds != null)
            {
                var posIntegrationIds = Context.PosCharges.Where(c => criteria.ChargeIds.Contains(c.ChargeId))
                                                                    .Select(c => c.Channel!.ChannelProfile!.PosIntegrationId);
                query = query.Where(q => posIntegrationIds.Contains(q.Id));
            }

            if (criteria.SessionIds != null)
            {
                var posIntegrationIds = Context.Sessions.Where(c => criteria.SessionIds.Contains(c.Id))
                                                                    .Select(c => c.Channel!.ChannelProfile!.PosIntegrationId);
                query = query.Where(q => posIntegrationIds.Contains(q.Id));
            }

            if (criteria.Types != null)
                query = query.Where(r => criteria.Types.Contains(r.IntegrationType));

            if (criteria.IsDeleted.HasValue)
                query = query.Where(r => r.DeletedDate.HasValue == criteria.IsDeleted.Value);

            if (criteria.SyncStates != null)
                query = query.Where(x => criteria.SyncStates.Contains(x.SyncState));

            if (criteria.IsDiagnosticErrorsMuted.HasValue)
                query = query.Where(x => x.DiagnosticErrorsMuted == criteria.IsDiagnosticErrorsMuted.Value);

            if (criteria.HasQrCodes.HasValue)
                query = query.Where(x => x.ChannelProfiles!.SelectMany(p => p.Channels!).Any() == criteria.HasQrCodes.Value);

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}
