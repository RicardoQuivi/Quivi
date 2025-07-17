namespace Quivi.Application.Commands.Orders
{
    public class AddOrder
    {
        public required int ChannelId { get; init; }
        public required IEnumerable<AddOrderItem> Items { get; init; }
    }

    public record BaseAddOrderItem
    {
        public required int MenuItemId { get; init; }
        /// <summary>
        /// The items quantity. Positive value will add item, negative value will remove item.
        /// </summary>
        public decimal Quantity { get; init; }
        /// <summary>
        /// The item price override. Keep <c>null</c> if price is the original price.
        /// </summary>
        public decimal? Price { get; init; }
    }

    public record AddOrderItem : BaseAddOrderItem
    {
        public decimal Discount { get; init; }
        public required IEnumerable<BaseAddOrderItem> Extras { get; init; }
    }
}