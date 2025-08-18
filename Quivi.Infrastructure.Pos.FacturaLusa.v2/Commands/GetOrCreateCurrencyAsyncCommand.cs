using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Currencies;
using FacturaLusa.v2.Exceptions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class GetOrCreateCurrencyAsyncCommand : AFacturaLusaAsyncCommand<Currency>
    {
        private string _isoCode = string.Empty;
        public required string IsoCode
        {
            get => _isoCode;
            set => _isoCode = value.Trim();
        }

        public required string Name { get; init; }
        public required string Symbol { get; init; }
    }

    public class GetOrCreateCurrencyAsyncCommandHandler : ICommandHandler<GetOrCreateCurrencyAsyncCommand, Task<Currency>>
    {
        private readonly IFacturaLusaCacheProvider cacheProvider;

        public GetOrCreateCurrencyAsyncCommandHandler(IFacturaLusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<Currency> Handle(GetOrCreateCurrencyAsyncCommand command)
        {
            return await cacheProvider.GetOrCreateCurrency(
                command.Service.AccountUuid,
                command.IsoCode,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<Currency> EntityFactory(GetOrCreateCurrencyAsyncCommand command)
        {
            try
            {
                var response = await command.Service.CreateCurrency(new CreateCurrencyRequest
                {
                    Description = command.Name,
                    Symbol = command.Symbol,
                    IsoCode = command.IsoCode,
                    ExchangeSale = 1,
                    IsDefault = true,
                });
                return response;
            }
            catch (FacturaLusaException ex) when (ex.ErrorType == ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await command.Service.SearchCurrency(new SearchCurrencyRequest
                {
                    Value = command.IsoCode,
                });
                return existingResponse;
            }
        }
    }
}