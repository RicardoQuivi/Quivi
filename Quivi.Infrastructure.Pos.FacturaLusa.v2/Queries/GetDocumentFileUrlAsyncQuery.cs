using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Sales;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Queries
{
    public class GetDocumentFileUrlAsyncQuery : AFacturaLusaAsyncQuery<string>
    {
        public required string DocumentId { get; init; }
        public DocumentFormat DocumentFormat { get; init; }
    }

    public class GetDocumentFileUrlAsyncQueryHandler : IQueryHandler<GetDocumentFileUrlAsyncQuery, Task<string>>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IFacturaLusaCacheProvider cacheProvider;

        public GetDocumentFileUrlAsyncQueryHandler(IQueryProcessor queryProcessor, IFacturaLusaCacheProvider cacheProvider)
        {
            this.queryProcessor = queryProcessor;
            this.cacheProvider = cacheProvider;
        }

        public async Task<string> Handle(GetDocumentFileUrlAsyncQuery query)
        {
            long saleId = await queryProcessor.Execute(new GetFacturaLusaSaleIdAsyncQuery
            {
                Service = query.Service,
                DocumentId = query.DocumentId,
            });

            return await cacheProvider.GetOrCreateSaleDocumentUrl
            (
                query.Service.AccountUuid,
                saleId,
                query.DocumentFormat,
                async () =>
                {
                    var documentData = await query.Service.DownloadSaleFile(new DownloadSaleFileRequest
                    {
                        SaleId = saleId,
                        Issue = DocumentIssue.Original,
                        Format = query.DocumentFormat,
                    });
                    return documentData.Url;
                },
                TimeSpan.FromDays(7)
            );
        }
    }
}
