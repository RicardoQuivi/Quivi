using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPrinterWorkersRepository : ARepository<PrinterWorker, GetPrinterWorkersCriteria>, IPrinterWorkersRepository
    {
        public SqlPrinterWorkersRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PrinterWorker> GetFilteredQueryable(GetPrinterWorkersCriteria criteria)
        {
            IQueryable<PrinterWorker> query = Set;

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.Identifiers != null)
                query = query.Where(q => criteria.Identifiers.Contains(q.Identifier));

            if (criteria.IsDeleted.HasValue)
                query = query.Where(q => q.DeletedDate.HasValue == criteria.IsDeleted.HasValue);

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}