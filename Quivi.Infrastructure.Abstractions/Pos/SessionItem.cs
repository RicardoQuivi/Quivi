namespace Quivi.Infrastructure.Abstractions.Pos
{
    public record BaseSessionItem
    {
        public required int MenuItemId { get; init; }
        public decimal Quantity { get; init; }
        public decimal Price { get; init; }
    }

    public record SessionExtraItem : BaseSessionItem
    {
        public required int ModifierGroupId { get; init; }
    }

    public record SessionItem : BaseSessionItem
    {
        public decimal Discount { get; init; }
        public required IEnumerable<SessionExtraItem> Extras { get; init; }
    }
}