namespace Quivi.Guests.Api.Dtos.Requests.Transactions
{
    public class PayAtTheTableData
    {
        public IEnumerable<SessionItem>? Items { get; set; }
    }

    public class OrderAndPayData
    {
        public required string OrderId { get; set; }
    }

    public class CreateTransactionRequest : ARequest
    {
        public required string ChannelId { get; init; }
        public string? ConsumerPersonId { get; init; }
        public decimal Amount { get; init; }
        public decimal Tip { get; init; }
        public string? Email { get; init; }
        public string? VatNumber { get; init; }
        public required string MerchantAcquirerConfigurationId { get; init; }
        public PayAtTheTableData? PayAtTheTableData { get; init; }
        public OrderAndPayData? OrderAndPayData { get; init; }
    }
}