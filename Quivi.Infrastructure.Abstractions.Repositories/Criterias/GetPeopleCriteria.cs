using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Entities.Identity;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPeopleCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? SubMerchantIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<string>? Emails { get; init; }
        public bool? IsAnonymous { get; init; }
        public IEnumerable<BasicAuthClientType>? ClientTypes { get; init; }
        public IEnumerable<PersonType>? PersonTypes { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
