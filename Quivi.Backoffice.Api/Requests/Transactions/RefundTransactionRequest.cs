namespace Quivi.Backoffice.Api.Requests.Transactions
{
    public class RefundTransactionRequest
    {
        public decimal? Amount { get; init; }
        public bool Cancelation { get; init; }
    }
}