namespace Quivi.Backoffice.Api.Requests.MerchantDocuments
{
    public class GetMerchantDocumentsRequest : APagedRequest
    {
        public IEnumerable<string>? TransactionIds { get; init; }
        public bool MonthlyInvoiceOnly { get; init; }
    }
}