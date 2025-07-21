using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;

namespace Quivi.Infrastructure.Pos.Facturalusa.Queries
{
    public class GetDocumentFileAsyncQuery : AFacturalusaAsyncQuery<byte[]>
    {
        public GetDocumentFileAsyncQuery(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        public required string DocumentId { get; init; }
        public Models.Sales.DownloadSaleFormat DocumentFormat { get; init; }
    }

    public class GetDocumentFileAsyncQueryHandler : IQueryHandler<GetDocumentFileAsyncQuery, Task<byte[]>>
    {
        private readonly IQueryProcessor queryProcessor;

        public GetDocumentFileAsyncQueryHandler(IQueryProcessor queryProcessor)
        {
            this.queryProcessor = queryProcessor;
        }

        public async Task<byte[]> Handle(GetDocumentFileAsyncQuery query)
        {
            string fileUrl = await queryProcessor.Execute(new GetDocumentFileUrlAsyncQuery(query.FacturalusaService)
            {
                DocumentId = query.DocumentId,
                DocumentFormat = query.DocumentFormat,
            });

            using var httpClient = new HttpClient();
            var data = await httpClient.GetByteArrayAsync(fileUrl);
            return data;
        }
    }
}