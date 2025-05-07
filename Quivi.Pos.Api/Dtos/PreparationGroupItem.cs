namespace Quivi.Pos.Api.Dtos
{
    public record BasePreparationGroupItem
    {
        public required string Id { get; init; }
        public required string MenuItemId { get; init; }
        public int Quantity { get; init; }
        public int RemainingQuantity { get; init; }
        public string? LocationId { get; init; }
    }

    public record PreparationGroupItem : BasePreparationGroupItem
    {
        public required IEnumerable<BasePreparationGroupItem> Extras { get; init; }
    }
}