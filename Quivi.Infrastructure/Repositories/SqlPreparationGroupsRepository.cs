using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPreparationGroupsRepository : ARepository<PreparationGroup, GetPreparationGroupsCriteria>, IPreparationGroupsRepository
    {
        public SqlPreparationGroupsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PreparationGroup> GetFilteredQueryable(GetPreparationGroupsCriteria criteria)
        {
            IQueryable<PreparationGroup> query = Set;

            if (criteria.IncludePreparationGroupItems)
                query = query.Include(q => q.PreparationGroupItems);

            if (criteria.IncludePreparationGroupItemMenuItems)
                query = query.Include(q => q.PreparationGroupItems!).ThenInclude(i => i.MenuItem);

            if (criteria.IncludeSession)
                query = query.Include(q => q.Session);

            if (criteria.IncludeSessionChannel)
                query = query.Include(q => q.Session!).ThenInclude(q => q.Channel);

            if (criteria.IncludeSessionChannelProfile)
                query = query.Include(q => q.Session!).ThenInclude(q => q.Channel!).ThenInclude(q => q.ChannelProfile);

            if (criteria.IncludeOrders)
                query = query.Include(q => q.Orders);

            if (criteria.IncludeOrdersSequence)
                query = query.Include(q => q.Orders!).ThenInclude(q => q.OrderSequence);

            if (criteria.IncludeOrderFields)
                query = query.Include(q => q.Orders!).ThenInclude(o => o.OrderAdditionalInfos!).ThenInclude(a => a.OrderConfigurableField);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.ParentPreparationGroupIds != null)
                query = query.Where(q => q.ParentPreparationGroupId.HasValue && criteria.ParentPreparationGroupIds.Contains(q.ParentPreparationGroupId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.SessionIds != null)
                query = query.Where(q => criteria.SessionIds.Contains(q.SessionId));

            if (criteria.States != null)
                query = query.Where(q => criteria.States.Contains(q.State));

            if (criteria.Completed.HasValue)
                query = query.Where(q => q.PreparationGroupItems!.Any(i => i.RemainingQuantity != 0) == !criteria.Completed);

            if (criteria.LocationIds != null)
            {
                query = query.Where(q => q.PreparationGroupItems!.Where(item => item.LocationId.HasValue && criteria.LocationIds.Contains(item.LocationId.Value))
                                                                .Any());

                if (criteria.Completed.HasValue)
                    query = query.Where(q => q.PreparationGroupItems!.Any(item => item.ParentPreparationGroupItemId.HasValue) == false
                                            ||
                                                q.PreparationGroupItems!.Where(item => item.ParentPreparationGroupItemId.HasValue)
                                                                        .Where(item => item.LocationId.HasValue && criteria.LocationIds.Contains(item.LocationId.Value))
                                                                        .Any(i => i.RemainingQuantity != 0) == !criteria.Completed
                                            ||
                                                q.PreparationGroupItems!.Where(item => item.Extras!.Where(extra => extra.LocationId.HasValue && criteria.LocationIds.Contains(extra.LocationId.Value))
                                                                                                .Any())
                                                                        .Any(i => i.RemainingQuantity != 0) == !criteria.Completed);
            }

            if (criteria.FromCreatedDate.HasValue)
                query = query.Where(q => criteria.FromCreatedDate.Value <= q.CreatedDate);

            if (criteria.ToCreatedDate.HasValue)
                query = query.Where(q => q.CreatedDate <= criteria.ToCreatedDate.Value);

            if (criteria.OrderIds != null)
                query = query.Where(q => q.Orders!.Any(o => criteria.OrderIds.Contains(o.Id)));

            return query.OrderBy(o => o.CreatedDate);
        }
    }
}