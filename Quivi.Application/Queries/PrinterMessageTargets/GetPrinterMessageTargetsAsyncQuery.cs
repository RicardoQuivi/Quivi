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
        public IEnumerable<int>? PrinterNotificationMessageIds { get; init; }
        public bool? DeletedTargets { get; init; }
        public bool IncludePrinterNotificationMessage { get; init; }
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
                PrinterNotificationMessageIds = query.PrinterNotificationMessageIds,
                IncludePrinterNotificationMessage = query.IncludePrinterNotificationMessage,
                IncludePrinterNotificationsContactPrinterWorker = query.IncludePrinterNotificationsContactPrinterWorker,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
