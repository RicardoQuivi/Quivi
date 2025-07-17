using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlJournalsRepository : ARepository<Journal, GetJournalsCriteria>, IJournalsRepository
    {
        public SqlJournalsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Journal> GetFilteredQueryable(GetJournalsCriteria criteria)
        {
            IQueryable<Journal> query = Set;

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}