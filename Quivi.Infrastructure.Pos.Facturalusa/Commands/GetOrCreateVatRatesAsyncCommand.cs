using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Exceptions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class GetOrCreateVatRatesAsyncCommand : AFacturalusaAsyncCommand<IEnumerable<VatRate>>
    {
        public GetOrCreateVatRatesAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        public required IEnumerable<VatRateData> VatRates { get; set; }
    }

    public class VatRateData
    {
        private decimal _percentageValue;
        public decimal PercentageValue 
        {
            get => _percentageValue; 
            set => _percentageValue = Math.Round(value, 2);
        }
    }

    public class GetOrCreateVatRatesAsyncCommandHandler : ICommandHandler<GetOrCreateVatRatesAsyncCommand, Task<IEnumerable<VatRate>>>
    {
        private readonly IFacturalusaCacheProvider cacheProvider;

        public GetOrCreateVatRatesAsyncCommandHandler(IFacturalusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<IEnumerable<VatRate>> Handle(GetOrCreateVatRatesAsyncCommand command)
        {
            var result = new List<VatRate>();
            foreach (var vatRateData in command.VatRates)
            {
                var vatRateResult = await GetOrCreateVatRate(command.FacturalusaService, vatRateData);
                result.Add(vatRateResult);
            }
            return result;
        }

        private async Task<VatRate> GetOrCreateVatRate(IFacturalusaService service, VatRateData vatRate)
        {
            return await cacheProvider.GetOrCreateVatRate(
                service.AccountUuid,
                vatRate.PercentageValue,
                async () => await GetOrCreateVatRateViaApi(service, vatRate),
                TimeSpan.FromDays(7)
            );
        }

        private async Task<VatRate> GetOrCreateVatRateViaApi(IFacturalusaService service, VatRateData vatRate)
        {
            try
            {
                var response = await service.CreateVatRate(new CreateVatRateRequest
                {
                    Name = $"Taxa IVA {vatRate.PercentageValue}%",
                    PercentageTax = vatRate.PercentageValue,
                    TaxType = vatRate.PercentageValue > 0 ? VatRateTaxType.Normal : VatRateTaxType.Exempt,
                    IsActive = true,
                });
                return response.Data;
            }
            catch (FacturalusaApiException ex) when (ex.ErrorType == Models.ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await service.GetVatRates(new GetVatRatesRequest
                {
                    Value = vatRate.PercentageValue,
                });
                return existingResponse.Data.First(vr => vr.PercentageTax == vatRate.PercentageValue);
            }
        }
    }
}
