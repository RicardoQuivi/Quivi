namespace Quivi.Guests.Api.Dtos.Requests.Transactions
{
    public class GetTransactionsRequest : APagedRequest
    {
        public string? Id { get; init; }
        public string? SessionId { get; init; }
        public string? OrderId { get; init; }
    }
}