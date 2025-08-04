namespace Quivi.Backoffice.Api.Requests.Transactions
{
    public class GetTransactionsRequest : AGetTransactionsRequest, IPagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public int Page { get; init; }
        public int? PageSize { get; init; }
    }
}