using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.PrinterWorkers
{
    public class GetPrinterWorkersAsyncQuery : APagedAsyncQuery<PrinterWorker>
    {
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? Ids { get; init; }
    }

    public class GetPrinterWorkersAsyncQueryHandler : APagedQueryAsyncHandler<GetPrinterWorkersAsyncQuery, PrinterWorker>
    {
        private readonly IPrinterWorkersRepository repository;

        public GetPrinterWorkersAsyncQueryHandler(IPrinterWorkersRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<PrinterWorker>> Handle(GetPrinterWorkersAsyncQuery query)
        {
            return repository.GetAsync(new Infrastructure.Abstractions.Repositories.Criterias.GetPrinterWorkersCriteria
            {
                MerchantIds = query.MerchantIds,
                Ids = query.Ids,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}