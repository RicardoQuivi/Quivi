namespace Quivi.Pos.Api.Dtos.Requests.Transactions
{
    public class GetTransactionsRequest : AGetTransactionsRequest, IPagedRequest
    {
        public int Page { get; init; } = 0;
        public int? PageSize { get; init; } = null;
    }
}