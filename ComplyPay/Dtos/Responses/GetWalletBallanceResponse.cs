namespace ComplyPay.Dtos.Responses
{
    public class GetWalletBallanceResponse
    {
        public required string StatusMessage { get; init; }
        public required Balance Balance { get; init; }
    }
}