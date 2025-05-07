using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlApplicationUsersRepository : ARepository<ApplicationUser, GetApplicationUsersCriteria>, IApplicationUsersRepository
    {
        public SqlApplicationUsersRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<ApplicationUser> GetFilteredQueryable(GetApplicationUsersCriteria criteria)
        {
            IQueryable<ApplicationUser> query = Set;

            if (criteria.IncludeMerchants)
                query = query.Include(q => q.Merchants);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.MerchantIds != null)
                query = query.Where(q => q.Merchants!.Any(m => criteria.MerchantIds.Contains(m.Id)));

            return query.OrderBy(q => q.Id);
        }
    }
}
