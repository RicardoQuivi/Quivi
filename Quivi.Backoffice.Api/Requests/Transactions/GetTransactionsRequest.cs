namespace Quivi.Backoffice.Api.Requests.Transactions
{
    public class GetTransactionsRequest : AGetTransactionsRequest, IPagedRequest
    {
        public int Page { get; init; }
        public int? PageSize { get; init; }
    }
}