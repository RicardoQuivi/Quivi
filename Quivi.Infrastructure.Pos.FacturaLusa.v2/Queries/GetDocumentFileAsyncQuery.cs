using FacturaLusa.v2.Dtos;
using Quivi.Infrastructure.Abstractions.Cqrs;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Queries
{
    public class GetDocumentFileAsyncQuery : AFacturaLusaAsyncQuery<byte[]>
    {
        public required string DocumentId { get; init; }
        public DocumentFormat DocumentFormat { get; init; }
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
            string fileUrl = await queryProcessor.Execute(new GetDocumentFileUrlAsyncQuery
            {
                Service = query.Service,
                DocumentId = query.DocumentId,
                DocumentFormat = query.DocumentFormat,
            });

            using var httpClient = new HttpClient();
            var data = await httpClient.GetByteArrayAsync(fileUrl);
            return data;
        }
    }
}
