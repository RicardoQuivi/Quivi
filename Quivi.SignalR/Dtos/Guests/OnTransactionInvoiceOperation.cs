namespace Quivi.SignalR.Dtos.Guests
{
    public class OnTransactionInvoiceOperation
    {
        public required string MerchantId { get; init; }
        public required string Id { get; init; }
        public required string PosChargeId { get; init; }
    }
}