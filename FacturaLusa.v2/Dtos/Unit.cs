namespace FacturaLusa.v2.Dtos
{
    public class Unit
    {
        public long Id { get; init; }
        public required string Description { get; init; }
        public required string Symbol { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}