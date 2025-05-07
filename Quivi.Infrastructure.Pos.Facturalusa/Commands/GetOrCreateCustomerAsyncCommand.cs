using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Exceptions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Customers;

namespace Quivi.Infrastructure.Pos.Facturalusa.Commands
{
    public class GetOrCreateCustomerAsyncCommand : AFacturalusaAsyncCommand<ReadOnlyCustomer>
    {
        public GetOrCreateCustomerAsyncCommand(IFacturalusaService facturalusaService) : base(facturalusaService)
        {
        }

        public bool IsFinalConsumer { get; set; }
        public CustomerType Type { get; set; }
        public string? Code { get; set; }

        private string? _vatNumber = string.Empty;
        public string? VatNumber 
        {
            get => _vatNumber;
            set => _vatNumber = value?.Trim();
        }

        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
    }

    public class GetOrCreateCustomerAsyncCommandHandler : ICommandHandler<GetOrCreateCustomerAsyncCommand, Task<ReadOnlyCustomer>>
    {
        private readonly IFacturalusaCacheProvider cacheProvider;

        public GetOrCreateCustomerAsyncCommandHandler(IFacturalusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<ReadOnlyCustomer> Handle(GetOrCreateCustomerAsyncCommand command)
        {
            string? vatNumber = IsUnknownCustomer(command) ? ACustomer.DefaultCode : command.VatNumber;

            return await cacheProvider.GetOrCreateCustomer(
                command.FacturalusaService.AccountUuid,
                vatNumber ?? string.Empty,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private bool IsUnknownCustomer(GetOrCreateCustomerAsyncCommand command) => command.IsFinalConsumer || string.IsNullOrWhiteSpace(command.VatNumber);

        private async Task<ReadOnlyCustomer> EntityFactory(GetOrCreateCustomerAsyncCommand command)
        {
            // Final Consumer is Pre-Created in Facturalusa so we only need to get the consumer data
            if (IsUnknownCustomer(command))
            {
                var finalConsumerResponse = await command.FacturalusaService.GetCustomers(new GetCustomersRequest
                {
                    FilterBy = CustomerFilter.Code,
                    Value = ACustomer.DefaultCode,
                });
                return finalConsumerResponse.Data.Single();
            }

            try
            {
                // Create a new customer
                var newCustomerResponse = await command.FacturalusaService.CreateCustomer(new CreateCustomerRequest
                {
                    Code = string.IsNullOrWhiteSpace(command.Code) ? command.VatNumber : command.Code,
                    Name = string.IsNullOrWhiteSpace(command.Name) ? "Desconhecido" : command.Name,
                    VatNumber = command.VatNumber,
                    Email = command.Email,
                    MobileNumber = command.MobileNumber,
                    CountryName = string.IsNullOrWhiteSpace(command.CountryName) ? ACustomer.DefaultCountryName : command.CountryName,
                    CityName = string.IsNullOrWhiteSpace(command.CityName) ? ACustomer.DefaultCityName : command.CityName,
                    PostalCode = string.IsNullOrWhiteSpace(command.PostalCode) ? ACustomer.DefaultPostalCode : command.PostalCode,
                    Address = string.IsNullOrWhiteSpace(command.Address) ? ACustomer.DefaultAddress : command.Address,
                    Language = Models.Language.Portuguese,
                    Type = command.Type,
                    VatType = Models.VatRates.VatRateType.DoNothing,
                });
                return newCustomerResponse.Data;
            }
            catch (FacturalusaApiException ex) when (ex.ErrorType == Models.ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await command.FacturalusaService.GetCustomers(new GetCustomersRequest
                {
                    FilterBy = CustomerFilter.VatNumber,
                    Value = command.VatNumber,
                });
                return existingResponse.Data.First(c => c.VatNumber == command.VatNumber);
            }
        }
    }
}