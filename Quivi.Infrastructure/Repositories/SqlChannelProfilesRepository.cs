using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlChannelProfilesRepository : ARepository<ChannelProfile, GetChannelProfilesCriteria>, IChannelProfilesRepository
    {
        public SqlChannelProfilesRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<ChannelProfile> GetFilteredQueryable(GetChannelProfilesCriteria criteria)
        {
            IQueryable<ChannelProfile> query = Set;

            if (criteria.IncludeChannels)
                query = query.Include(q => q.Channels);

            if (criteria.IncludeAssociatedOrderConfigurableFields)
                query = query.Include(q => q.AssociatedOrderConfigurableFields);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.ChannelIds != null)
                query = query.Where(q => q.Channels!.Any(c => criteria.ChannelIds.Contains(c.Id)));

            if (criteria.ParentMerchantIds != null)
                query = query.Where(q => q.Merchant!.ParentMerchantId.HasValue && criteria.ParentMerchantIds.Contains(q.Merchant.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.PreparationGroupIds != null)
                query = query.Where(q => q.Channels!.SelectMany(qr => qr.Sessions!)
                                                        .SelectMany(s => s.PreparationGroups!)
                                                        .Any(s => criteria.PreparationGroupIds.Contains(s.Id)));

            if (criteria.Flags.HasValue)
                query = query.Where(q => q.Features.HasFlag(criteria.Flags.Value));

            if (criteria.HasChannels != null)
                query = query.Where(q => q.Channels!.Any() == criteria.HasChannels.Value);

            if (criteria.IsDeleted != null)
                query = query.Where(q => q.DeletedDate.HasValue == criteria.IsDeleted.Value);

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}
