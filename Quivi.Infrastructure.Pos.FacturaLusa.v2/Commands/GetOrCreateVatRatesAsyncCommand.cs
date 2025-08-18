using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.VatRates;
using FacturaLusa.v2.Exceptions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class VatRateData
    {
        private decimal _percentageValue;
        public decimal PercentageValue
        {
            get => _percentageValue;
            set => _percentageValue = Math.Round(value, 2);
        }
    }

    public class GetOrCreateVatRatesAsyncCommand : AFacturaLusaAsyncCommand<IEnumerable<VatRate>>
    {
        public required IEnumerable<VatRateData> VatRates { get; init; }
    }

    public class GetOrCreateVatRatesAsyncCommandHandler : ICommandHandler<GetOrCreateVatRatesAsyncCommand, Task<IEnumerable<VatRate>>>
    {
        private readonly IFacturaLusaCacheProvider cacheProvider;

        public GetOrCreateVatRatesAsyncCommandHandler(IFacturaLusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<IEnumerable<VatRate>> Handle(GetOrCreateVatRatesAsyncCommand command)
        {
            var result = new List<VatRate>();
            foreach (var vatRateData in command.VatRates)
            {
                var vatRateResult = await GetOrCreateVatRate(command.Service, vatRateData);
                result.Add(vatRateResult);
            }
            return result;
        }

        private Task<VatRate> GetOrCreateVatRate(IFacturaLusaService service, VatRateData vatRate)
        {
            return cacheProvider.GetOrCreateVatRate(
                service.AccountUuid,
                vatRate.PercentageValue,
                () => GetOrCreateVatRateViaApi(service, vatRate),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<VatRate> GetOrCreateVatRateViaApi(IFacturaLusaService service, VatRateData vatRate)
        {
            try
            {
                var response = await service.CreateVatRate(new CreateVatRateRequest
                {
                    Description = $"Taxa IVA {vatRate.PercentageValue}%",
                    TaxPercentage = vatRate.PercentageValue,
                    Type = vatRate.PercentageValue > 0 ? TaxType.Normal : TaxType.Isenta,
                });
                return response;
            }
            catch (FacturaLusaException ex) when (ex.ErrorType == ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await service.SearchVatRate(new SearchVatRateRequest
                {
                    Value = vatRate.PercentageValue.ToString(),
                });
                return existingResponse;
            }
        }
    }
}
