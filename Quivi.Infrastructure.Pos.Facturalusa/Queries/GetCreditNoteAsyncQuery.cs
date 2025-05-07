using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Mappers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.External;

namespace Quivi.Infrastructure.Pos.Facturalusa.Queries
{
    public class GetCreditNoteAsyncQuery : AFacturalusaAsyncQuery<CreditNote>
    {
        public GetCreditNoteAsyncQuery(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        public required string DocumentId { get; init; }
    }

    public class GetCreditNoteAsyncQueryHandler : IQueryHandler<GetCreditNoteAsyncQuery, Task<CreditNote>>
    {
        private readonly IQueryProcessor queryProcessor;

        public GetCreditNoteAsyncQueryHandler(IQueryProcessor queryProcessor)
        {
            this.queryProcessor = queryProcessor;
        }

        public async Task<CreditNote> Handle(GetCreditNoteAsyncQuery query)
        {
            long saleId = await queryProcessor.Execute(new GetFacturalusaSaleIdAsyncQuery(query.FacturalusaService)
            {
                DocumentId = query.DocumentId,
            });

            var response = await query.FacturalusaService.GetSale(new Models.Sales.GetSalesRequest 
            { 
                FilterBy = Models.Sales.SaleFilter.Id,
                Value = saleId,
            });

            return SaleMapper.ConvertToCreditNote(response.Data);
        }
    }
}