using System.ComponentModel.DataAnnotations;

namespace Quivi.Pos.Api.Dtos.Requests.Orders
{
    public class CreateOrdersRequest : ARequest
    {
        public required IEnumerable<CreateOrder> Orders { get; init; }
    }

    public class CreateOrder
    {
        public required string ChannelId { get; init; }
        public required IEnumerable<OrderItem> Items { get; init; }
    }

    public class BaseOrderItem
    {
        public required string MenuItemId { get; init; }

        /// <summary>
        /// The items quantity. Positive value will add item, negative value will remove item.
        /// </summary>
        public decimal Quantity { get; init; }

        /// <summary>
        /// The item price override. Keep <c>null</c> if price is the original price.
        /// </summary>
        public decimal? Price { get; init; }
    }

    public class OrderItem : BaseOrderItem
    {
        public decimal Discount { get; init; }
        public required IEnumerable<BaseOrderItem> Extras { get; init; } = [];
    }
}
