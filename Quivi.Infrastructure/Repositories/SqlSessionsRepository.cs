using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlSessionsRepository : ARepository<Session, GetSessionsCriteria>, ISessionsRepository
    {
        public SqlSessionsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Session> GetFilteredQueryable(GetSessionsCriteria criteria)
        {
            IQueryable<Session> query = Set;

            if (criteria.IncludeOrdersMenuItems)
                query = query.Include(q => q.Orders!).ThenInclude(o => o.OrderMenuItems);

            if (criteria.IncludeOrdersMenuItemsPosChargeInvoiceItems)
                query = query.Include(q => q.Orders!).ThenInclude(o => o.OrderMenuItems!).ThenInclude(o => o.PosChargeInvoiceItems);

            if (criteria.IncludeChannel)
                query = query.Include(q => q.Channel!);

            if (criteria.IncludeOrdersMenuItemsModifiers)
                query = query.Include(q => q.Orders!).ThenInclude(o => o.OrderMenuItems!).ThenInclude(o => o.Modifiers);

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.Channel!.MerchantId));

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.ChannelIds != null)
                query = query.Where(q => criteria.ChannelIds.Contains(q.ChannelId));

            if (criteria.PosIntegrationIds != null)
                query = query.Where(q => criteria.PosIntegrationIds.Contains(q.Channel!.ChannelProfile!.PosIntegrationId));

            if (criteria.Statuses != null)
                query = query.Where(q => criteria.Statuses.Contains(q.Status));

            if (criteria.LatestSessionsOnly)
            {
                //We should refactor this. We should have a table "ActiveSession" in which a
                //Channel could only have one active (Open) session. Then we could make a more efficient search.
                //In this refactor, SessinState would disappear, and we would also have a Soft Delete solumn instead of Unknown status.

                var status = criteria.Statuses ?? [SessionStatus.Closed, SessionStatus.Ordering];
                var sessionIds = Set.GroupBy(q => q.ChannelId).Select(g => g.OrderByDescending(s => s.CreateDate)
                                                                                            .Where(s => status.Contains(s.Status))
                                                                                            .FirstOrDefault())
                                                                            .Where(s => s != null)
                                                                            .Select(s => s.Id);
                query = query.Where(q => sessionIds.Contains(q.Id));
            }

            return query.OrderByDescending(q => q.Id);
        }
    }
}
