namespace Quivi.Guests.Api.Dtos.Requests.Transactions
{
    public class PutPaybyrdCreditCardTransactionRequest : PutTransactionRequest
    {
        public required string TokenId { get; init; }
        public required string RedirectUrl { get; init; }
    }
}