using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Customers;
using FacturaLusa.v2.Exceptions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands
{
    public class GetOrCreateCustomerAsyncCommand : AFacturaLusaAsyncCommand<Customer>
    {
        public bool IsFinalConsumer { get; set; }
        public CustomerType Type { get; set; }
        public string? Code { get; set; }

        private string _vatNumber = string.Empty;
        public required string VatNumber
        {
            get => _vatNumber;
            set => _vatNumber = value.Trim();
        }

        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
    }

    public class GetOrCreateCustomerAsyncCommandHandler : ICommandHandler<GetOrCreateCustomerAsyncCommand, Task<Customer>>
    {
        public static readonly string DefaultCode = "Consumidor Final";
        public static string DefaultAddress = "Desconhecido";
        public static string DefaultPostalCode = "0000-000";
        public static string DefaultCityName = "Desconhecido";
        public static string DefaultCountryName = "Portugal";

        private readonly IFacturaLusaCacheProvider cacheProvider;

        public GetOrCreateCustomerAsyncCommandHandler(IFacturaLusaCacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public async Task<Customer> Handle(GetOrCreateCustomerAsyncCommand command)
        {
            string? vatNumber = IsUnknownCustomer(command) ? DefaultCode : command.VatNumber;

            return await cacheProvider.GetOrCreateCustomer(
                command.Service.AccountUuid,
                vatNumber ?? string.Empty,
                () => EntityFactory(command),
                TimeSpan.FromDays(7)
            );
        }

        private bool IsUnknownCustomer(GetOrCreateCustomerAsyncCommand command) => command.IsFinalConsumer || string.IsNullOrWhiteSpace(command.VatNumber);

        private async Task<Customer> EntityFactory(GetOrCreateCustomerAsyncCommand command)
        {
            // Final Consumer is Pre-Created in Facturalusa so we only need to get the consumer data
            if (IsUnknownCustomer(command))
            {
                var finalConsumerResponse = await command.Service.SearchCustomer(new SearchCustomerRequest
                {
                    SearchField = SearchField.Code,
                    Value = DefaultCode,
                });
                return finalConsumerResponse;
            }

            try
            {
                // Create a new customer
                var newCustomerResponse = await command.Service.CreateCustomer(new CreateCustomerRequest
                {
                    Code = string.IsNullOrWhiteSpace(command.Code) ? command.VatNumber : command.Code,
                    Name = string.IsNullOrWhiteSpace(command.Name) ? "Desconhecido" : command.Name,
                    VatNumber = command.VatNumber,
                    Email = command.Email,
                    MobilePhone = command.MobileNumber,
                    Country = string.IsNullOrWhiteSpace(command.CountryName) ? DefaultCountryName : command.CountryName,
                    City = string.IsNullOrWhiteSpace(command.CityName) ? DefaultCityName : command.CityName,
                    PostalCode = string.IsNullOrWhiteSpace(command.PostalCode) ? DefaultPostalCode : command.PostalCode,
                    Address = string.IsNullOrWhiteSpace(command.Address) ? DefaultAddress : command.Address,
                    Type = command.Type,
                    VatType = VatType.NoAction,
                });
                return newCustomerResponse;
            }
            catch (FacturaLusaException ex) when (ex.ErrorType == ErrorType.FieldValueAlreadyExists)
            {
                var existingResponse = await command.Service.SearchCustomer(new SearchCustomerRequest
                {
                    SearchField = SearchField.VatNumber,
                    Value = command.VatNumber,
                });
                return existingResponse;
            }
        }
    }
}
