using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlOrderConfigurableFieldChannelProfileAssociationsRepository : ARepository<OrderConfigurableFieldChannelProfileAssociation, GetOrderConfigurableFieldChannelProfileAssociationsCriteria>, IOrderConfigurableFieldChannelProfileAssociationsRepository
    {
        public SqlOrderConfigurableFieldChannelProfileAssociationsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<OrderConfigurableFieldChannelProfileAssociation> GetFilteredQueryable(GetOrderConfigurableFieldChannelProfileAssociationsCriteria criteria)
        {
            IQueryable<OrderConfigurableFieldChannelProfileAssociation> query = Set;

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.ChannelProfile!.MerchantId) || criteria.MerchantIds.Contains(q.OrderConfigurableField!.MerchantId));

            if (criteria.ChannelProfileIds != null)
                query = query.Where(q => criteria.ChannelProfileIds.Contains(q.ChannelProfileId));

            if (criteria.OrderConfigurableFieldIds != null)
                query = query.Where(q => criteria.OrderConfigurableFieldIds.Contains(q.OrderConfigurableFieldId));

            return query.OrderBy(o => o.CreatedDate);
        }
    }
}