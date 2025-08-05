namespace Paybyrd.Api.Models
{
    public class Order
    {
        public DateTime OrderDate { get; init; }
        public required OrderStatus Status { get; init; }
        public required string CheckoutUrl { get; init; }
        public required string OrderId { get; init; }
        public required string Amount { get; init; }
        public required Currency Currency { get; init; }
        public required string OrderRef { get; init; }
        public required OrderOptions OrderOptions { get; init; }
        public required string CheckoutKey { get; init; }
        public string? LastTransactionId { get; init; }
    }
}