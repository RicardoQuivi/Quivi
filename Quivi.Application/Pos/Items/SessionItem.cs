namespace Quivi.Application.Pos.Items
{
    public record BaseSessionItem
    {
        public required int MenuItemId { get; init; }
        public decimal Quantity { get; init; }
        public decimal Price { get; init; }
    }

    public record SessionItem : BaseSessionItem
    {
        public decimal Discount { get; init; }
        public required IEnumerable<BaseSessionItem> Extras { get; init; }
    }

    public record PaidSessionItem : SessionItem
    {
        public decimal PaidQuantity { get; init; }
    }
}