using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;

namespace Quivi.Infrastructure.Pos.Facturalusa.Queries
{
    public abstract class AFacturalusaAsyncQuery<T> : IQuery<Task<T>>
    {
        public IFacturalusaService FacturalusaService { get; }

        public AFacturalusaAsyncQuery(IFacturalusaService facturalusaService)
        {
            FacturalusaService = facturalusaService;
        }
    }
}