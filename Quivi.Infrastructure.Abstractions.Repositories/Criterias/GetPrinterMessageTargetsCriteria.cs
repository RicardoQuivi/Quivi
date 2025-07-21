namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPrinterMessageTargetsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? PrinterNotificationMessageIds { get; init; }
        public IEnumerable<int>? PrinterNotificationsContactIds { get; init; }
        public bool? DeletedTargets { get; init; }
        public bool IncludePrinterNotificationMessage { get; init; }
        public bool IncludePrinterNotificationsContact { get; init; }
        public bool IncludePrinterNotificationsContactBaseNotificationsContact { get; init; }
        public bool IncludePrinterNotificationsContactPrinterWorker { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}