namespace FacturaLusa.v2.Dtos
{
    public class Currency
    {
        public long Id { get; init; }
        public required string Description { get; init; }
        public required string Symbol { get; init; }
        public required string IsoCode { get; init; }
        public required decimal ExchangeRate { get; init; }
        public bool IsDefault { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}