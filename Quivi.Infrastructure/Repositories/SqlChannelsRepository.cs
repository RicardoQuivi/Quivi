using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlChannelsRepository : ARepository<Channel, GetChannelsCriteria>, IChannelsRepository
    {
        public SqlChannelsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Channel> GetFilteredQueryable(GetChannelsCriteria criteria)
        {
            IQueryable<Channel> query = Set;

            if(criteria.IncludeChannelProfile)
                query = query.Include(q => q.ChannelProfile);

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.ChannelProfileIds != null)
                query = query.Where(x => criteria.ChannelProfileIds.Contains(x.ChannelProfileId));

            if (criteria.SessionIds != null)
                query = query.Where(x => x.Sessions!.Any(s => criteria.SessionIds.Contains(s.Id)));

            if (criteria.Identifiers != null)
                query = query.Where(x => criteria.Identifiers.Contains(x.Identifier));

            if (criteria.Flags.HasValue)
                query = query.Where(q => q.ChannelProfile!.Features.HasFlag(criteria.Flags.Value));

            if (criteria.IsDeleted.HasValue)
                query = query.Where(x => x.DeletedDate.HasValue == criteria.IsDeleted.Value);

            if (!string.IsNullOrWhiteSpace(criteria.Search))
                query = query.Where(x => EF.Functions.Collate(x.Identifier, "Latin1_General_CI_AI").Contains(criteria.Search));

            if (criteria.HasOpenSession.HasValue)
                query = query.Where(q => q.Sessions!.Any(s => s.Status == SessionStatus.Ordering) == criteria.HasOpenSession.Value);

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}
