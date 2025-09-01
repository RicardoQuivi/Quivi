using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlEmployeesRepository : ARepository<Employee, GetEmployeesCriteria>, IEmployeesRepository
    {
        public SqlEmployeesRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Employee> GetFilteredQueryable(GetEmployeesCriteria criteria)
        {
            IQueryable<Employee> query = Set;

            if (criteria.IncludeMerchant)
                query = query.Include(e => e.Merchant);

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