namespace FacturaLusa.v2.Dtos.Requests.PaymentConditions
{
    public class CreatePaymentConditionRequest
    {
        public required string Description { get; init; }
        public string? EnglishDescription { get; init; }
        public int Days { get; init; }
    }
}