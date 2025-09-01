namespace Quivi.Pos.Api.Dtos.Requests.Transactions
{
    public abstract class AGetTransactionsRequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}