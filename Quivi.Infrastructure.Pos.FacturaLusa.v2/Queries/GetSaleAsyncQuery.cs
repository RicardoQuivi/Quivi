using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Sales;
using Quivi.Infrastructure.Abstractions.Cqrs;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Queries
{
    public class GetSaleAsyncQuery : AFacturaLusaAsyncQuery<Sale>
    {
        public required string DocumentId { get; init; }
    }

    public class GetSaleAsyncQueryHandler : IQueryHandler<GetSaleAsyncQuery, Task<Sale>>
    {
        private readonly IQueryProcessor queryProcessor;

        public GetSaleAsyncQueryHandler(IQueryProcessor queryProcessor)
        {
            this.queryProcessor = queryProcessor;
        }

        public async Task<Sale> Handle(GetSaleAsyncQuery query)
        {
            long saleId = await queryProcessor.Execute(new GetFacturaLusaSaleIdAsyncQuery
            {
                Service = query.Service,
                DocumentId = query.DocumentId,
            });

            var response = await query.Service.SearchSale(new SearchSaleRequest
            {
                Field = SearchField.Id,
                Value = saleId.ToString(),
            });

            return response;
        }
    }
}