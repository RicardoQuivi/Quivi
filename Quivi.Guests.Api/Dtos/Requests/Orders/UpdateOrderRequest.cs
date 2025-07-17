namespace Quivi.Guests.Api.Dtos.Requests.Orders
{
    public class UpdateOrderRequest
    {
        public required string Id { get; init; }
        public required IEnumerable<OrderItem> Items { get; init; }
        public IReadOnlyDictionary<string, string>? Fields { get; init; }
    }
}