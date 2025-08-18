using FacturaLusa.v2.Dtos;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Models
{
    public record SaleDocumentCustomer
    {
        public required string VatNumber { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? PostalCode { get; init; }
        public string? Country { get; init; }
        public required CustomerType Type { get; init; }
        public required bool IsFinalConsumer { get; init; }
        public string? Code { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public string? MobileNumber { get; init; }
    }
}