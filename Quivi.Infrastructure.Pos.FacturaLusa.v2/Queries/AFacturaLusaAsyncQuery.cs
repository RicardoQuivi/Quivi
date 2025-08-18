using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Queries
{
    public abstract class AFacturaLusaAsyncQuery<T> : IQuery<Task<T>>
    {
        public required IFacturaLusaService Service { get; init; }
    }
}