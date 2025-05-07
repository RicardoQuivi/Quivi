using Quivi.Infrastructure.Pos.Facturalusa.Models.Customers;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.External
{
    public class Customer
    {
        public bool IsFinalConsumer { get; set; }
        public CustomerType Type { get; set; }
        public string? Code { get; set; }
        public string? VatNumber { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? MobileNumber { get; set; }
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
    }
}