namespace Quivi.Guests.Api.Dtos.Requests.Transactions
{
    public class PutPaybyrdMbWayTransactionRequest : PutTransactionRequest
    {
        public required string TokenId { get; init; }
        public string? RedirectUrl { get; init; }
    }
}