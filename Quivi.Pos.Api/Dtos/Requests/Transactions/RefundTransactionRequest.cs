namespace Quivi.Pos.Api.Dtos.Requests.Transactions
{
    public class RefundTransactionRequest
    {
        public decimal? Amount { get; init; }
        public bool Cancelation { get; init; }
        public string? Reason { get; init; }
    }
}