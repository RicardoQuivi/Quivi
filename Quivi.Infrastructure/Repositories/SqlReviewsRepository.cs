using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlReviewsRepository : ARepository<Review, GetReviewsCriteria>, IReviewsRepository
    {
        public SqlReviewsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Review> GetFilteredQueryable(GetReviewsCriteria criteria)
        {
            IQueryable<Review> query = Set;

            if (criteria.PosChargeIds != null)
                query = query.Where(q => criteria.PosChargeIds.Contains(q.PosChargeId));

            return query.OrderBy(q => q.PosChargeId);
        }
    }
}