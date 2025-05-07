using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Mappers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.External;

namespace Quivi.Infrastructure.Pos.Facturalusa.Queries
{
    public class GetConsumerBillReceiptAsyncQuery : AFacturalusaAsyncQuery<ConsumerBill>
    {
        public GetConsumerBillReceiptAsyncQuery(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        public required string DocumentId { get; init; }
    }

    public class GetBudgetReceiptAsyncQueryHandler : IQueryHandler<GetConsumerBillReceiptAsyncQuery, Task<ConsumerBill>>
    {
        private readonly IQueryProcessor queryProcessor;

        public GetBudgetReceiptAsyncQueryHandler(IQueryProcessor queryProcessor)
        {
            this.queryProcessor = queryProcessor;
        }

        public async Task<ConsumerBill> Handle(GetConsumerBillReceiptAsyncQuery query)
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

            return SaleMapper.ConvertToBudgetReceipt(response.Data);
        }
    }
}
