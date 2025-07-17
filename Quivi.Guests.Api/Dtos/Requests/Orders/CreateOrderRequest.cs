namespace Quivi.Guests.Api.Dtos.Requests.Orders
{
    public class CreateOrderRequest : ARequest
    {
        public required string ChannelId { get; init; }
        public required IEnumerable<OrderItem> Items { get; init; }
    }

    public class OrderItem : BaseOrderItem
    {
        public IEnumerable<OrderItemModifierGroup>? ModifierGroups { get; init; }
    }

    public class BaseOrderItem
    {
        public required string MenuItemId { get; init; }
        public int Quantity { get; init; }
    }

    public class OrderItemModifierGroup
    {
        public required string ModifierId { get; init; }
        public required IEnumerable<BaseOrderItem> SelectedOptions { get; init; }
    }
}