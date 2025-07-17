using Quivi.Domain.Entities.Notifications;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetPrinterNotificationsContactsCriteria : IPagedCriteria
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? PrinterWorkerIds { get; init; }
        public IEnumerable<int>? LocationIds { get; set; }
        public IEnumerable<NotificationMessageType>? MessageTypes { get; init; }

        public bool? IsDeleted { get; init; }
        public bool IncludeNotificationsContact { get; init; }
        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}