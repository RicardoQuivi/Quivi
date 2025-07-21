using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PrinterMessageTargets
{
    public class GetPrinterMessageTargetsAsyncQuery : APagedAsyncQuery<PrinterMessageTarget>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? PrinterNotificationsContactIds { get; init; }
        public IEnumerable<int>? PrinterNotificationMessageIds { get; init; }
        public bool? DeletedTargets { get; init; }
        public bool IncludePrinterNotificationMessage { get; init; }
        public bool IncludePrinterNotificationsContact { get; init; }
        public bool IncludePrinterNotificationsContactBaseNotificationsContact { get; init; }
        public bool IncludePrinterNotificationsContactPrinterWorker { get; init; }
    }

    public class GetPrinterMessageTargetsAsyncQueryHandler : IQueryHandler<GetPrinterMessageTargetsAsyncQuery, Task<IPagedData<PrinterMessageTarget>>>
    {
        private readonly IPrinterMessageTargetsRepository repository;

        public GetPrinterMessageTargetsAsyncQueryHandler(IPrinterMessageTargetsRepository repository)
        {
            this.repository = repository;
        }

        public Task<IPagedData<PrinterMessageTarget>> Handle(GetPrinterMessageTargetsAsyncQuery query)
        {
            return repository.GetAsync(new GetPrinterMessageTargetsCriteria
            {
                MerchantIds = query.MerchantIds,
                PrinterNotificationMessageIds = query.PrinterNotificationMessageIds,
                PrinterNotificationsContactIds = query.PrinterNotificationsContactIds,
                IncludePrinterNotificationMessage = query.IncludePrinterNotificationMessage,
                IncludePrinterNotificationsContact = query.IncludePrinterNotificationsContact,
                IncludePrinterNotificationsContactBaseNotificationsContact = query.IncludePrinterNotificationsContactBaseNotificationsContact,
                IncludePrinterNotificationsContactPrinterWorker = query.IncludePrinterNotificationsContactPrinterWorker,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
