namespace FacturaLusa.v2.Dtos
{
    public class PaymentCondition
    {
        public long Id { get; init; }
        public required string Description { get; init; }
        public string? EnglishDescription { get; init; }
        public int Days { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}