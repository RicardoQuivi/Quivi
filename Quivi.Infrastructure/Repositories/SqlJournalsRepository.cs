using Microsoft.EntityFrameworkCore;
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

            if (criteria.IncludeJournalDetails)
                query = query.Include(p => p.JournalDetails);

            if (criteria.IncludeMerchantFees)
                query = query.Include(p => p.Postings!)
                                .ThenInclude(o => o.Person!)
                                .ThenInclude(x => x.ParentMerchant!)
                                .ThenInclude(x => x.Fees);

            if (criteria.IncludeSubMerchantFees)
                query = query.Include(p => p.Postings!)
                                .ThenInclude(o => o.Person!)
                                .ThenInclude(x => x.Merchant!)
                                .ThenInclude(x => x.Fees);

            if (criteria.States != null)
                query = query.Where(x => criteria.States.Contains(x.State));

            if (criteria.Types != null)
                query = query.Where(x => criteria.Types.Contains(x.Type));

            if (criteria.FromDate.HasValue)
                query = query.Where(q => criteria.FromDate.Value <= q.CreatedDate);

            if (criteria.ToDate.HasValue)
                query = query.Where(q => q.CreatedDate < criteria.ToDate.Value);

            if (criteria.OrderRefs != null)
                query = query.Where(q => criteria.OrderRefs.Any(r => r == q.OrderRef));

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}