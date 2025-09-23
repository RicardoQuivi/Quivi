using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.ChannelProfiles
{
    public class GetChannelProfilesAsyncQuery : APagedAsyncQuery<ChannelProfile>
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? PreparationGroupIds { get; init; }
        public ChannelFeature? Flags { get; init; }
        public bool? HasChannels { get; init; }
        public bool? IsDeleted { get; init; } = false;
    }

    public class GetChannelProfilesAsyncQueryHandler : APagedQueryAsyncHandler<GetChannelProfilesAsyncQuery, ChannelProfile>
    {
        private readonly IChannelProfilesRepository repository;

        public GetChannelProfilesAsyncQueryHandler(IChannelProfilesRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<ChannelProfile>> Handle(GetChannelProfilesAsyncQuery query)
        {
            return repository.GetAsync(new GetChannelProfilesCriteria
            {
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                ChannelIds = query.ChannelIds,
                Ids = query.Ids,
                PreparationGroupIds = query.PreparationGroupIds,
                Flags = query.Flags,
                HasChannels = query.HasChannels,
                IsDeleted = query.IsDeleted,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
