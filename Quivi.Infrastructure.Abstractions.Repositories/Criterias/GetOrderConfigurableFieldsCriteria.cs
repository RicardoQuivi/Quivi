namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetOrderConfigurableFieldsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChannelsIds { get; init; }
        public IEnumerable<int>? ChannelProfileIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<string>? Names { get; init; }
        public bool? ForPosSessions { get; init; }
        public bool? ForOrdering { get; init; }
        public bool? IsAutoFill { get; init; }
        public bool? IsDeleted { get; init; }

        public bool IncludeTranslations { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}