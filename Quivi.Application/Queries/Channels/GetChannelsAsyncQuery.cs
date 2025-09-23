using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Channels
{
    public class GetChannelsAsyncQuery : APagedAsyncQuery<Channel>
    {
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? SessionIds { get; init; }
        public IEnumerable<int>? ChannelProfileIds { get; init; }
        public ChannelFeature? Flags { get; init; }
        public string? Search { get; set; }
        public bool? HasOpenSession { get; set; }
        public bool? IsDeleted { get; init; } = false;

        public bool IncludeChannelProfile { get; set; }
    }

    public class GetChannelsAsyncQueryHandler : APagedQueryAsyncHandler<GetChannelsAsyncQuery, Channel>
    {
        private readonly IChannelsRepository repository;

        public GetChannelsAsyncQueryHandler(IChannelsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Channel>> Handle(GetChannelsAsyncQuery query)
        {
            return repository.GetAsync(new GetChannelsCriteria
            {
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                SessionIds = query.SessionIds,
                ChannelProfileIds = query.ChannelProfileIds,
                Flags = query.Flags,
                Search = query.Search,
                HasOpenSession = query.HasOpenSession,
                IsDeleted = query.IsDeleted,

                IncludeChannelProfile = query.IncludeChannelProfile,

                PageSize = query.PageSize,
                PageIndex = query.PageIndex,
            });
        }
    }
}