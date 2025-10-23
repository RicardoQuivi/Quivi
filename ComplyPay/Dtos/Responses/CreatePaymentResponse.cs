namespace ComplyPay.Dtos.Responses
{
    public class CreatePaymentResponse
    {
        public int Id { get; init; }
        public required string StatusMessage { get; init; }
    }
}