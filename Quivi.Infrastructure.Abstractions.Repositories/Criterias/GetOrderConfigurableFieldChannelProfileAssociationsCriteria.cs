namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetOrderConfigurableFieldChannelProfileAssociationsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? OrderConfigurableFieldIds { get; init; }
        public IEnumerable<int>? ChannelProfileIds { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}