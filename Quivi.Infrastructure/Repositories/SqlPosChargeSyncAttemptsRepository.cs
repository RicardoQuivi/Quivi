using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPosChargeSyncAttemptsRepository : ARepository<PosChargeSyncAttempt, GetPosChargeSyncAttemptsCriteria>, IPosChargeSyncAttemptsRepository
    {
        public SqlPosChargeSyncAttemptsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PosChargeSyncAttempt> GetFilteredQueryable(GetPosChargeSyncAttemptsCriteria criteria)
        {
            IQueryable<PosChargeSyncAttempt> query = Set;

            if (criteria.IncludePosCharge)
                query = query.Include(q => q.PosCharge);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.PosChargeIds != null)
                query = query.Where(q => criteria.PosChargeIds.Contains(q.PosChargeId));

            if (criteria.States != null)
                query = query.Where(q => criteria.States.Contains(q.State));

            if (criteria.Types != null)
                query = query.Where(q => criteria.Types.Contains(q.Type));

            return query.OrderBy(o => o.Id);
        }
    }
}