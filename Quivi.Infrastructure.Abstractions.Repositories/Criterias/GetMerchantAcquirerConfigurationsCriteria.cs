using Quivi.Domain.Entities.Charges;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetMerchantAcquirerConfigurationsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<ChargeMethod>? ChargeMethods { get; init; }
        public IEnumerable<ChargePartner>? ChargePartners { get; init; }
        public IEnumerable<string>? ApiKeys { get; init; }
        public bool? IsDeleted { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}