using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlChargesRepository : ARepository<Charge, GetChargesCriteria>, IChargesRepository
    {
        public SqlChargesRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Charge> GetFilteredQueryable(GetChargesCriteria criteria)
        {
            IQueryable<Charge> query = Set;

            if (criteria.IncludeDeposit)
                query = query.Include(q => q.Deposit);

            if (criteria.IncludeDepositCaptureJournal)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositCaptureJournal!).ThenInclude(q => q.Journal);

            if (criteria.IncludeDepositDepositCapture)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositCapture);

            if (criteria.IncludeDepositCaptureJournal)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositSurchage!);

            if (criteria.IncludeDepositSurchargeJournal)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositSurchargeJournal);

            if (criteria.IncludePosCharge)
                query = query.Include(q => q.PosCharge!);

            if (criteria.IncludeDepositConsumer)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.Consumer);

            if (criteria.IncludeChainedCharge)
                query = query.Include(q => q.ChainedCharge);

            if (criteria.IncludeDepositDepositCapturePerson)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositCapture!).ThenInclude(q => q.Person);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if(criteria.Statuses != null)
                query = query.Where(q => criteria.Statuses.Contains(q.Status));

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}