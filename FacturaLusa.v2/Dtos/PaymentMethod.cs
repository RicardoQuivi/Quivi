namespace FacturaLusa.v2.Dtos
{
    public class PaymentMethod
    {
        public long Id { get; init; }
        public required string Description { get; init; }
        public string? EnglishDescription { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}