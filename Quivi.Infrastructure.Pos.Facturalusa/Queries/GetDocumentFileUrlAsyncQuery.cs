using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;

namespace Quivi.Infrastructure.Pos.Facturalusa.Queries
{
    public class GetDocumentFileUrlAsyncQuery : AFacturalusaAsyncQuery<string>
    {
        public GetDocumentFileUrlAsyncQuery(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        public required string DocumentId { get; init; }
        public Models.Sales.DownloadSaleFormat DocumentFormat { get; init; }
    }

    public class GetDocumentFileUrlAsyncQueryHandler : IQueryHandler<GetDocumentFileUrlAsyncQuery, Task<string>>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IFacturalusaCacheProvider cacheProvider;

        public GetDocumentFileUrlAsyncQueryHandler(IQueryProcessor queryProcessor, IFacturalusaCacheProvider cacheProvider)
        {
            this.queryProcessor = queryProcessor;
            this.cacheProvider = cacheProvider;
        }

        public async Task<string> Handle(GetDocumentFileUrlAsyncQuery query)
        {
            long saleId = await queryProcessor.Execute(new GetFacturalusaSaleIdAsyncQuery(query.FacturalusaService)
            {
                DocumentId = query.DocumentId,
            });

            return await cacheProvider.GetOrCreateSaleDocumentUrl
            (
                query.FacturalusaService.AccountUuid,
                saleId,
                query.DocumentFormat,
                async () =>
                {
                    var documentData = await query.FacturalusaService.DownloadSale(saleId, new Models.Sales.DownloadSaleRequest
                    {
                        Issue = Models.Sales.DownloadSaleIssue.Original,
                        Format = query.DocumentFormat,
                    });
                    return documentData.FileUrl;
                },
                TimeSpan.FromDays(7)
            );
        }
    }
}