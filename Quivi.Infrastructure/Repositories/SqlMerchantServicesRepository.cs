using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlMerchantServicesRepository : ARepository<MerchantService, GetMerchantServicesCriteria>, IMerchantServicesRepository
    {
        public SqlMerchantServicesRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<MerchantService> GetFilteredQueryable(GetMerchantServicesCriteria criteria)
        {
            IQueryable<MerchantService> query = Set;

            if (criteria.IncludeMerchants)
                query = query.Include(s => s.Merchant);

            if (criteria.IncludePostings)
                query = query.Include(s => s.Person)
                                .ThenInclude(s => s.Postings);

            if (criteria.IncludeJournals)
                query = query.Include(s => s.Person!)
                                .ThenInclude(s => s.Postings!)
                                .ThenInclude(p => p.Journal!);

            if (criteria.PersonIds != null)
                query = query.Where(s => criteria.PersonIds.Any(e => e == s.PersonId));

            if (criteria.MerchantIds != null)
                query = query.Where(s => criteria.MerchantIds.Any(e => e == s.MerchantId));

            if (criteria.MerchantServiceTypes != null)
                query = query.Where(s => criteria.MerchantServiceTypes.Any(e => e == s.Type));

            if (criteria.UnpaidOnly.HasValue)
                query = query.Where(s => (s.Person!.Postings!.Sum(p => p.Amount) != 0) == criteria.UnpaidOnly.Value);

            if (criteria.PaymentsFrom.HasValue)
                query = query.Where(s => s.Person!.Postings!.Any(p => criteria.PaymentsFrom.Value <= p.Journal!.CreatedDate));

            if (criteria.PaymentsTo.HasValue)
                query = query.Where(s => s.Person!.Postings!.Any(p => p.Journal!.CreatedDate <= criteria.PaymentsTo.Value));

            return query.OrderByDescending(q => q.PersonId);
        }
    }
}
