using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public abstract class AFacturaLusaAsyncCommand : ICommand<Task>
    {
        public required IFacturaLusaService Service { get; init; }
    }

    public abstract class AFacturaLusaAsyncCommand<T> : ICommand<Task<T>>
    {
        public required IFacturaLusaService Service { get; init; }
    }
}