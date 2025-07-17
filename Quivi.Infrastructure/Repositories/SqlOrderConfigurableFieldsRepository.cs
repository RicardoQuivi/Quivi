using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlOrderConfigurableFieldsRepository : ARepository<OrderConfigurableField, GetOrderConfigurableFieldsCriteria>, IOrderConfigurableFieldsRepository
    {
        public SqlOrderConfigurableFieldsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<OrderConfigurableField> GetFilteredQueryable(GetOrderConfigurableFieldsCriteria criteria)
        {
            IQueryable<OrderConfigurableField> query = Set;

            if (criteria.IncludeTranslations)
                query = query.Include(q => q.Translations);

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.ChannelProfile.MerchantId));

            if (criteria.ChannelsIds != null)
            {
                var profileIds = Context.Channels.Where(q => criteria.ChannelsIds.Contains(q.Id)).Select(q => q.ChannelProfileId);
                query = query.Where(q => profileIds.Contains(q.ChannelProfileId));
            }

            if (criteria.ChannelProfileIds != null)
                query = query.Where(q => criteria.ChannelProfileIds.Contains(q.ChannelProfileId));

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.Names != null)
                query = query.Where(q => criteria.Names.Contains(q.Name));

            if (criteria.ForPosSessions.HasValue)
                query = query.Where(q => q.AssignedOn.HasFlag(AssignedOn.PoSSessions) == criteria.ForPosSessions.Value);

            if (criteria.ForOrdering.HasValue)
                query = query.Where(q => q.AssignedOn.HasFlag(AssignedOn.Ordering) == criteria.ForOrdering.Value);

            if (criteria.IsAutoFill.HasValue)
                query = query.Where(q => q.IsAutoFill == criteria.IsAutoFill.Value);

            if (criteria.IsDeleted.HasValue)
                query = query.Where(q => q.DeletedDate.HasValue == criteria.IsDeleted.Value);

            return query.OrderBy(o => o.Id);
        }
    }
}