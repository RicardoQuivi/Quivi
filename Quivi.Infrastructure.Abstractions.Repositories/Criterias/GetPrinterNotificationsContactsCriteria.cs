namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPrinterNotificationsContactsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public bool IncludeNotificationsContact { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
