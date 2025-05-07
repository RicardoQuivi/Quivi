using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlLocationsRepository : ARepository<Location, GetLocationsCriteria>, ILocationsRepository
    {
        public SqlLocationsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Location> GetFilteredQueryable(GetLocationsCriteria criteria)
        {
            IQueryable<Location> query = Set;

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.Names != null)
                query = query.Where(q => criteria.Names.Contains(q.Name));

            if (criteria.IsDeleted != null)
                query = query.Where(q => q.DeletedDate.HasValue == criteria.IsDeleted.Value);

            return query.OrderBy(q => q.Id);
        }
    }
}