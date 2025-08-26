using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlOrderAdditionalInfosRepository : ARepository<OrderAdditionalInfo, GetOrderAdditionalInfosCriteria>, IOrderAdditionalInfosRepository
    {
        public SqlOrderAdditionalInfosRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<OrderAdditionalInfo> GetFilteredQueryable(GetOrderAdditionalInfosCriteria criteria)
        {
            IQueryable<OrderAdditionalInfo> query = Set;

            if (criteria.IncludeOrderConfigurableField)
                query = query.Include(q => q.OrderConfigurableField);

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.Order!.MerchantId));

            if (criteria.SessionIds != null)
                query = query.Where(q => q.Order!.SessionId.HasValue && criteria.SessionIds.Contains(q.Order!.SessionId.Value));

            if (criteria.OrderConfigurableFieldIds != null)
                query = query.Where(q => criteria.OrderConfigurableFieldIds.Contains(q.OrderConfigurableFieldId));

            if (criteria.PrintedOn != null)
                query = query.Where(q => criteria.PrintedOn.Contains(q.OrderConfigurableField!.PrintedOn));

            if (criteria.AssignedOn != null)
                query = query.Where(q => criteria.AssignedOn.Contains(q.OrderConfigurableField!.AssignedOn));

            return query.OrderBy(q => q.OrderId);
        }
    }
}