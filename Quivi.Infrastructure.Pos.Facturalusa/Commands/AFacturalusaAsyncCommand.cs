using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public abstract class AFacturalusaAsyncCommand : ICommand<Task>
    {
        public IFacturalusaService FacturalusaService { get; }

        public AFacturalusaAsyncCommand(IFacturalusaService facturalusaService)
        {
            FacturalusaService = facturalusaService;
        }
    }

    public abstract class AFacturalusaAsyncCommand<T> : ICommand<Task<T>>
    {
        public IFacturalusaService FacturalusaService { get; }

        public AFacturalusaAsyncCommand(IFacturalusaService facturalusaService)
        {
            FacturalusaService = facturalusaService;
        }
    }
}