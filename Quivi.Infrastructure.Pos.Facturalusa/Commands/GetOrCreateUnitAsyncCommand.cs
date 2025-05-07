using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Exceptions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Units;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class GetOrCreateUnitAsyncCommand : AFacturalusaAsyncCommand<Unit>
    {
        public GetOrCreateUnitAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        private string _name = string.Empty;
        public required string Name 
        {
            get => _name;
            set => _name = value.Trim();
        }

        public required string Symbol { get; set; }
    }

    public class GetOrCreateUnitAsyncCommandHander : ICommandHandler<GetOrCreateUnitAsyncCommand, Task<Unit>>
    {
        private readonly IFacturalusaCacheProvider cacheProvider;

        public GetOrCreateUnitAsyncCommandHander(IFacturalusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<Unit> Handle(GetOrCreateUnitAsyncCommand command)
        {
            return await cacheProvider.GetOrCreateUnit
            (
                command.FacturalusaService.AccountUuid,
                command.Name,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<Unit> EntityFactory(GetOrCreateUnitAsyncCommand command)
        {
            try
            {
                var response = await command.FacturalusaService.CreateUnit(new CreateUnitRequest
                {
                    Name = command.Name,
                    Symbol = command.Symbol,
                    IsActive = true,
                });
                return response.Data;
            }
            catch (FacturalusaApiException ex) when (ex.ErrorType == Models.ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await command.FacturalusaService.GetUnits(new GetUnitsRequest
                {
                    Name = command.Name,
                });
                return existingResponse.Data.First(u => string.Equals(u.Name, command.Name, System.StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
