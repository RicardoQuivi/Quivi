namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPrinterNotificationMessagesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public bool IncludePrinterMessageTargets { get; init; }
        public bool IncludePrinterMessageTargetsPrinterNotificationsContact { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}
