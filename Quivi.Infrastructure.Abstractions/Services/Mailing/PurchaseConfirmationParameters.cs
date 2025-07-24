namespace Quivi.Infrastructure.Abstractions.Services.Mailing
{
    public class PurchaseConfirmationParameters
    {
        public required string MerchantName { get; init; }
        public required DateTime Date { get; init; }
        public required decimal Amount { get; init; }
        public required string TransactionId { get; init; }
    }
}