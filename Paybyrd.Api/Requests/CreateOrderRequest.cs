using Paybyrd.Api.Models;

namespace Paybyrd.Api.Requests
{
    public class Shopper
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
    }

    public class CreateOrderRequest
    {
        public int IsoAmount { get; init; }
        public Currency Currency { get; init; }
        public required string OrderRef { get; init; }
        public required Shopper Shopper { get; init; }
        public required OrderOptions OrderOptions { get; init; }
        public IEnumerable<string>? AllowedPaymentMethods { get; init; }
    }
}