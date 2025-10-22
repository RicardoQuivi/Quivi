using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPeopleRepository : ARepository<Person, GetPeopleCriteria>, IPeopleRepository
    {
        public SqlPeopleRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Person> GetFilteredQueryable(GetPeopleCriteria criteria)
        {
            IQueryable<Person> query = Set;

            if (criteria.IncludePostings)
                query = query.Include(q => q.Postings);

            if (criteria.ParentMerchantIds != null)
                query = query.Where(e => e.ParentMerchantId.HasValue && criteria.ParentMerchantIds.Contains(e.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(e => e.MerchantId.HasValue && criteria.MerchantIds.Contains(e.MerchantId.Value));

            if (criteria.ChannelIds != null)
                query = query.Where(e => e.Merchant!.Channels!.Any(channel => criteria.ChannelIds.Contains(channel.Id)));

            if (criteria.Ids != null)
                query = query.Where(p => criteria.Ids.Contains(p.Id));

            if (criteria.Emails != null)
            {
                var userIds = Context.Users.Where(u => criteria.Emails.Contains(u.Email)).Select(u => u.Id);
                query = query.Where(p => p.UserId.HasValue && userIds.Contains(p.UserId.Value));
            }

            if (criteria.ClientTypes != null)
                query = query.Where(e => e.ApiClients!.Any(ac => criteria.ClientTypes.Contains(ac.ClientType)));

            if (criteria.PersonTypes != null)
                query = query.Where(e => criteria.PersonTypes.Contains(e.PersonType));

            if (criteria.IsAnonymous.HasValue)
                query = query.Where(p => p.IsAnonymous == criteria.IsAnonymous.Value);

            if (criteria.HasMerchantService.HasValue)
                query = query.Where(p => (p.MerchantService == null) == criteria.HasMerchantService.Value);

            return query.OrderBy(p => p.Id);
        }
    }
}