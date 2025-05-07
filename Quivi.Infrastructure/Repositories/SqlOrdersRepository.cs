using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlOrdersRepository : ARepository<Order, GetOrdersCriteria>, IOrdersRepository
    {
        public SqlOrdersRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Order> GetFilteredQueryable(GetOrdersCriteria criteria)
        {
            IQueryable<Order> query = Set;

            if (criteria.IncludeOrderMenuItems)
                query = query.Include(x => x.OrderMenuItems);

            if (criteria.IncludeOrderMenuItemsPosChargeInvoiceItems)
                query = query.Include(x => x.OrderMenuItems!).ThenInclude(o => o.PosChargeInvoiceItems);

            if (criteria.IncludeOrderMenuItemsAndMofifiers)
                query = query.Include(x => x.OrderMenuItems!).ThenInclude(o => o.Modifiers);

            if (criteria.IncludeChangeLogs)
                query = query.Include(x => x.OrderChangeLogs);

            if (criteria.IncludeChannel)
                query = query.Include(x => x.Channel);

            if (criteria.IncludeChannelProfile)
                query = query.Include(x => x.Channel!).ThenInclude(c => c.ChannelProfile);

            if (criteria.IncludeChannelProfilePosIntegration)
                query = query.Include(q => q.Channel)
                                .ThenInclude(q => q!.ChannelProfile)
                                .ThenInclude(q => q!.PosIntegration);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.SessionIds != null)
                query = query.Where(q => q.SessionId.HasValue && criteria.SessionIds.Contains(q.SessionId.Value));

            if (criteria.States != null)
                query = query.Where(q => criteria.States.Contains(q.State));

            if (criteria.AssociatedWithPreparationGroup.HasValue)
                query = query.Where(q => q.PreparationGroups!.Any() == criteria.AssociatedWithPreparationGroup.Value);

            if (criteria.AssociatedWithSession.HasValue)
                query = query.Where(q => q.SessionId.HasValue == criteria.AssociatedWithSession.Value);

            return query.OrderBy(o => o.Id);
        }
    }
}
