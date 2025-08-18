namespace FacturaLusa.v2.Dtos.Requests.Items
{
    public class CreateItemRequest
    {
        public string? Reference { get; init; }
        public required string Description { get; init; }
        public string? Details { get; init; }
        public int? CategoryId { get; init; }
        public required long UnitId { get; init; }
        public required long VatRateId { get; init; }
        public required ItemType Type { get; init; }
        public string? Observations { get; init; }
    }
}