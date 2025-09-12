namespace Quivi.Pos.Api.Dtos
{
    public class BaseSessionItem
    {
        public required string MenuItemId { get; init; }
        public decimal Quantity { get; init; }
        public decimal Price { get; init; }
        public decimal OriginalPrice { get; init; }
    }

    public class SessionExtraItem : BaseSessionItem
    {
        public required string ModifierGroupId { get; init; }
    }

    public class SessionItem : BaseSessionItem
    {
        public required string Id { get; init; }
        public decimal DiscountPercentage { get; init; }
        public bool IsPaid { get; init; }
        public required IEnumerable<SessionExtraItem> Extras { get; init; }
        public DateTimeOffset LastModified { get; init; }
    }
}