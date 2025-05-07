namespace Quivi.Pos.Api.Dtos.Requests.Transactions
{
    public class GetTransactionsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}