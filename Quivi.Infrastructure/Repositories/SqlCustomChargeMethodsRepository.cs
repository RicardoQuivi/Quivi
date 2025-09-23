using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlCustomChargeMethodsRepository : ARepository<CustomChargeMethod, GetCustomChargeMethodsCriteria>, ICustomChargeMethodsRepository
    {
        public SqlCustomChargeMethodsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<CustomChargeMethod> GetFilteredQueryable(GetCustomChargeMethodsCriteria criteria)
        {
            IQueryable<CustomChargeMethod> query = Set;

            if (criteria.ParentMerchantIds != null)
                query = query.Where(e => e.Merchant!.ParentMerchantId.HasValue && criteria.ParentMerchantIds.Contains(e.Merchant.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(e => criteria.MerchantIds.Contains(e.MerchantId));

            if (criteria.Ids != null)
                query = query.Where(e => criteria.Ids.Contains(e.Id));

            if (criteria.Names != null)
                query = query.Where(e => criteria.Names.Contains(e.Name));

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}