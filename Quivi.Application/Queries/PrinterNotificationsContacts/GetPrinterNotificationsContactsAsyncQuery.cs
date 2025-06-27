using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PrinterNotificationsContacts
{
    public class GetPrinterNotificationsContactsAsyncQuery : APagedAsyncQuery<PrinterNotificationsContact>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? PrinterWorkerIds { get; init; }
        public bool IncludeNotificationsContact { get; init; }
    }

    public class GetPrinterNotificationsContactsAsyncQueryHandler : APagedQueryAsyncHandler<GetPrinterNotificationsContactsAsyncQuery, PrinterNotificationsContact>
    {
        private readonly IPrinterNotificationsContactsRepository repository;

        public GetPrinterNotificationsContactsAsyncQueryHandler(IPrinterNotificationsContactsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<PrinterNotificationsContact>> Handle(GetPrinterNotificationsContactsAsyncQuery query)
        {
            return repository.GetAsync(new Infrastructure.Abstractions.Repositories.Criterias.GetPrinterNotificationsContactsCriteria
            {
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                PrinterWorkerIds = query.PrinterWorkerIds,
                IncludeNotificationsContact = query.IncludeNotificationsContact,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
