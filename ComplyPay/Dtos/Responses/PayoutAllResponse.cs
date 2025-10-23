namespace ComplyPay.Dtos.Responses
{
    public class PayoutAllResponse
    {
        public required string StatusMessage { get; init; }
        public required IEnumerable<int> PayoutIds { get; init; }
    }
}