using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Sales;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Queries
{
    public class GetFacturaLusaSaleIdAsyncQuery : AFacturaLusaAsyncQuery<long>
    {
        public required string DocumentId { get; set; }
    }

    public class GetFacturaLusaSaleIdAsyncQueryHandler : IQueryHandler<GetFacturaLusaSaleIdAsyncQuery, Task<long>>
    {
        private readonly IFacturaLusaCacheProvider cacheProvider;

        public GetFacturaLusaSaleIdAsyncQueryHandler(IFacturaLusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public Task<long> Handle(GetFacturaLusaSaleIdAsyncQuery query)
        {
            return cacheProvider.GetOrCreateSaleId(query.Service.AccountUuid, query.DocumentId, async () =>
            {
                var document = await query.Service.SearchSale(new SearchSaleRequest
                {
                    Field = SearchField.DocumentNumber,
                    Value = query.DocumentId,
                });
                return document.Id;
            }, TimeSpan.FromDays(30));
        }
    }
}
