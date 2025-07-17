using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlOrderSequencesRepository : ARepository<OrderSequence, GetOrderSequencesCriteria>, IOrderSequencesRepository
    {
        public SqlOrderSequencesRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<OrderSequence> GetFilteredQueryable(GetOrderSequencesCriteria criteria)
        {
            IQueryable<OrderSequence> query = Set;

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.Order!.MerchantId));

            return query.OrderByDescending(q => q.CreatedDate);
        }
    }
}
