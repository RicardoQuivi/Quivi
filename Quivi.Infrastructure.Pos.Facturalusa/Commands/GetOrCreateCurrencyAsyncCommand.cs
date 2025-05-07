using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Exceptions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Currencies;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class GetOrCreateCurrencyAsyncCommand : AFacturalusaAsyncCommand<Currency>
    {
        public GetOrCreateCurrencyAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        private string? _isoCode;
        public string? IsoCode 
        {
            get => _isoCode;
            set => _isoCode = value?.Trim();
        }

        public required string Name { get; init; }
        public required string Symbol { get; init; }
    }

    public class GetOrCreateCurrencyAsyncCommandHandler : ICommandHandler<GetOrCreateCurrencyAsyncCommand, Task<Currency>>
    {
        private readonly IFacturalusaCacheProvider _cacheProvider;

        public GetOrCreateCurrencyAsyncCommandHandler(IFacturalusaCacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }
     
        public async Task<Currency> Handle(GetOrCreateCurrencyAsyncCommand command)
        {
            return await _cacheProvider.GetOrCreateCurrency(
                command.FacturalusaService.AccountUuid, 
                command.IsoCode, 
                () => EntityFactory(command), 
                TimeSpan.FromDays(7)
            );
        }

        private async Task<Currency> EntityFactory(GetOrCreateCurrencyAsyncCommand command)
        {
            try
            {
                var response = await command.FacturalusaService.CreateCurrency(new CreateCurrencyRequest
                {
                    Name = command.Name,
                    Symbol = command.Symbol,
                    IsoCode = command.IsoCode,
                    Exchange = 1,
                    Active = true,
                });
                return response.Data;
            }
            catch (FacturalusaApiException ex) when (ex.ErrorType == Models.ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await command.FacturalusaService.GetCurrencies(new GetCurrenciesRequest
                {
                    Value = command.IsoCode,
                });
                return existingResponse.Data.First(c => string.Equals(c.IsoCode, command.IsoCode, System.StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
