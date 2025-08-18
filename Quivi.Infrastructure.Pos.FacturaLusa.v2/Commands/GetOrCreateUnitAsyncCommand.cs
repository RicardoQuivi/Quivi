using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Units;
using FacturaLusa.v2.Exceptions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class GetOrCreateUnitAsyncCommand : AFacturaLusaAsyncCommand<Unit>
    {
        private string name = string.Empty;
        public required string Name
        {
            get => name;
            init => name = value.Trim();
        }

        public required string Symbol { get; set; }
    }

    public class GetOrCreateUnitAsyncCommandHander : ICommandHandler<GetOrCreateUnitAsyncCommand, Task<Unit>>
    {
        private readonly IFacturaLusaCacheProvider cacheProvider;

        public GetOrCreateUnitAsyncCommandHander(IFacturaLusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<Unit> Handle(GetOrCreateUnitAsyncCommand command)
        {
            return await cacheProvider.GetOrCreateUnit
            (
                command.Service.AccountUuid,
                command.Name,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<Unit> EntityFactory(GetOrCreateUnitAsyncCommand command)
        {
            try
            {
                var response = await command.Service.CreateUnit(new CreateUnitRequest
                {
                    Description = command.Name,
                    Symbol = command.Symbol,
                });
                return response;
            }
            catch (FacturaLusaException ex) when (ex.ErrorType == ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await command.Service.SearchUnit(new SearchUnitRequest
                {
                    Value = command.Name,
                });
                return existingResponse;
            }
        }
    }
}
