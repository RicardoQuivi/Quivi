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

            if (criteria.IncludeDepositCaptureJournalPostings)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositCaptureJournal!).ThenInclude(q => q.Journal!).ThenInclude(q => q.Postings);

            if (criteria.IncludeDepositCaptureJournalPostingsPerson)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositCaptureJournal!).ThenInclude(q => q.Journal!).ThenInclude(q => q.Postings!).ThenInclude(q => q.Person);

            if (criteria.IncludeDepositCaptureJournalChanges)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositCaptureJournal!).ThenInclude(q => q.Journal!).ThenInclude(q => q.JournalChanges);

            if (criteria.IncludeDepositDepositCapture)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositCapture);

            if (criteria.IncludeDepositDepositSurchage)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositSurchage!);

            if (criteria.IncludeDepositSurchargeJournal)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositSurchargeJournal);

            if (criteria.IncludePosCharge)
                query = query.Include(q => q.PosCharge!);

            if (criteria.IncludePosChargeMerchant)
                query = query.Include(q => q.PosCharge!).ThenInclude(q => q.Merchant);

            if (criteria.IncludeDepositConsumer)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.Consumer);

            if (criteria.IncludeChainedCharge)
                query = query.Include(q => q.ChainedCharge);

            if (criteria.IncludeDepositDepositCapturePerson)
                query = query.Include(q => q.Deposit!).ThenInclude(q => q.DepositCapture!).ThenInclude(q => q.Person);

            if (criteria.IncludeCardCharge)
                query = query.Include(q => q.CardCharge);

            if (criteria.IncludeMerchantAcquirerConfiguration)
                query = query.Include(q => q.MerchantAcquirerConfiguration);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.PosCharge!.MerchantId));

            if (criteria.Statuses != null)
                query = query.Where(q => criteria.Statuses.Contains(q.Status));

            if (criteria.Methods != null)
                query = query.Where(q => criteria.Methods.Contains(q.ChargeMethod));

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}