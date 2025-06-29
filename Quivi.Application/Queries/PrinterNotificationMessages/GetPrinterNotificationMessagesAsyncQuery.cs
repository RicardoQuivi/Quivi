using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PrinterNotificationMessages
{
    public class GetPrinterNotificationMessagesAsyncQuery : APagedAsyncQuery<PrinterNotificationMessage>
    {
        public IEnumerable<int>? Ids { get; init; }
        public bool IncludePrinterMessageTargets { get; init; }
        public bool IncludePrinterMessageTargetsPrinterNotificationsContact { get; init; }
    }

    public class GetPrinterNotificationMessagesAsyncQueryHandler : IQueryHandler<GetPrinterNotificationMessagesAsyncQuery, Task<IPagedData<PrinterNotificationMessage>>>
    {
        private readonly IPrinterNotificationMessagesRepository repository;

        public GetPrinterNotificationMessagesAsyncQueryHandler(IPrinterNotificationMessagesRepository repository)
        {
            this.repository = repository;
        }

        public Task<IPagedData<PrinterNotificationMessage>> Handle(GetPrinterNotificationMessagesAsyncQuery query)
        {
            return repository.GetAsync(new GetPrinterNotificationMessagesCriteria
            {
                Ids = query.Ids,
                IncludePrinterMessageTargets = query.IncludePrinterMessageTargets,
                IncludePrinterMessageTargetsPrinterNotificationsContact = query.IncludePrinterMessageTargetsPrinterNotificationsContact,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
