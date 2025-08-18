namespace FacturaLusa.v2.Dtos
{
    public class Serie
    {
        public long Id { get; init; }
        public required string Description { get; init; }
        public long ValidUntilYear { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}