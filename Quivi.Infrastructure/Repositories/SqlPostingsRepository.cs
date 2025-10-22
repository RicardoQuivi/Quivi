using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPostingsRepository : ARepository<Posting, GetPostingsCriteria>, IPostingsRepository
    {
        public SqlPostingsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Posting> GetFilteredQueryable(GetPostingsCriteria criteria)
        {
            IQueryable<Posting> query = Set;

            if (criteria.IncludeJournal)
                query = query.Include(q => q.Journal);

            if (criteria.IncludeJournalSettlementServiceDetails)
                query = query.Include(q => q.Journal!).ThenInclude(q => q.SettlementServiceDetails);

            if (criteria.PersonIds != null)
                query = query.Where(q => criteria.PersonIds.Contains(q.PersonId));

            if (criteria.JournalOrderRefs != null)
                query = query.Where(q => criteria.JournalOrderRefs.Contains(q.Journal!.OrderRef));

            if (criteria.SettlementStartDate.HasValue)
                query = query.Where(q => q.Journal!.SettlementDetails!.Any(sd => criteria.SettlementStartDate.Value <= sd.Settlement!.Date) || q.Journal.SettlementServiceDetails!.Any(sd => criteria.SettlementStartDate.Value <= sd.Settlement!.Date));

            if (criteria.SettlementEndDate.HasValue)
                query = query.Where(q => q.Journal!.SettlementDetails!.Any(sd => sd.Settlement!.Date <= criteria.SettlementEndDate.Value) || q.Journal.SettlementServiceDetails!.Any(sd => sd.Settlement!.Date <= criteria.SettlementEndDate.Value));


            return query.OrderBy(q => q.CreatedDate);
        }

        public async Task<IReadOnlyDictionary<Person, decimal>> GetBalanceAsync(GetAccountBalanceCriteria criteria)
        {
            IQueryable<Posting> query = Set.Include(p => p.Person).Where(p => p.Journal!.State == JournalState.Completed);

            if (criteria.PersonIds != null)
                query = query.Where(p => criteria.PersonIds.Contains(p.PersonId));

            if (criteria.FromDate.HasValue)
                query = query.Where(p => criteria.FromDate.Value <= p.Journal!.CreatedDate);

            if (criteria.ToDate.HasValue)
                query = query.Where(p => p.Journal!.CreatedDate < criteria.ToDate);

            if (criteria.PhoneNumber != null)
                query = query.Where(p => p.Person!.PhoneNumber == criteria.PhoneNumber);

            if (criteria.JournalTypes != null)
                query = query.Where(p => criteria.JournalTypes.Contains(p.Journal!.Type));

            var result = await query.GroupBy(p => p.Person!)
                                     .Select(p => new
                                     {
                                         Person = p.Key,
                                         Balance = p.Sum(p1 => (decimal?)p1.Amount) ?? 0,
                                     })
                                     .ToDictionaryAsync(p => p.Person, p => p.Balance);
            return result;
        }
    }
}