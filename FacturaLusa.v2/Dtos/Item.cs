namespace FacturaLusa.v2.Dtos
{
    public class Item
    {
        public long Id { get; init; }
        public required string Reference { get; init; }
        public required string Description { get; init; }
        public string? Details { get; init; }
        public required Unit Unit { get; init; }
        public required VatRate VatRate { get; init; }
        public required ItemType Type { get; init; }
        public string? Observations { get; init; }
    }
}