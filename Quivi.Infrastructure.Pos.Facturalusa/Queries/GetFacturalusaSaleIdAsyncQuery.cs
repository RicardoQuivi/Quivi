using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;

namespace Quivi.Infrastructure.Pos.Facturalusa.Queries
{
    public class GetFacturalusaSaleIdAsyncQuery : AFacturalusaAsyncQuery<long>
    {
        public GetFacturalusaSaleIdAsyncQuery(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }
        
        /// <summary>
        /// Document ID used in Degrazie queries. This ID is not known by Facturalusa.
        /// </summary>
        public string DocumentId { get; set; }
    }

    public class GetFacturalusaSaleIdAsyncQueryHandler : IQueryHandler<GetFacturalusaSaleIdAsyncQuery, Task<long>>
    {
        private readonly IFacturalusaCacheProvider _cacheProvider;

        public GetFacturalusaSaleIdAsyncQueryHandler(IFacturalusaCacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public async Task<long> Handle(GetFacturalusaSaleIdAsyncQuery query)
        {
            return await _cacheProvider.GetOrCreateSaleId(query.FacturalusaService.AccountUuid, query.DocumentId, async () =>
            {
                var document = await query.FacturalusaService.GetSale(new Models.Sales.GetSalesRequest
                {
                    FilterBy = Models.Sales.SaleFilter.DocumentNumber,
                    Value = query.DocumentId,
                });
                return document.Data.Id;
            }, TimeSpan.FromDays(30));
        }
    }
}
