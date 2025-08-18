namespace FacturaLusa.v2.Dtos.Requests.PaymentMethods
{
    public class CreatePaymentMethodRequest
    {
        public required string Description { get; init; }
        public string? EnglishDescription { get; init; }
    }
}