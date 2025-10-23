using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlSettlementServiceDetailsRepository : ARepository<SettlementServiceDetail, GetSettlementServiceDetailsCriteria>, ISettlementServiceDetailsRepository
    {
        public SqlSettlementServiceDetailsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<SettlementServiceDetail> GetFilteredQueryable(GetSettlementServiceDetailsCriteria criteria)
        {
            IQueryable<SettlementServiceDetail> query = Set;

            if (criteria.SettlementIds != null)
                query = query.Where(q => criteria.SettlementIds.Contains(q.SettlementId));

            if (criteria.SettlementStates != null)
                query = query.Where(q => criteria.SettlementStates.Contains(q.Settlement!.State));

            if (criteria.IsMerchantDemo.HasValue)
                query = query.Where(q => q.Merchant!.IsDemo == criteria.IsMerchantDemo.Value);

            return query.OrderByDescending(q => q.JournalId);
        }
    }
}