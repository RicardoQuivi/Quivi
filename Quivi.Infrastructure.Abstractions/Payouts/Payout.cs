namespace Quivi.Infrastructure.Abstractions.Payouts
{
    public class Payout
    {
        public required string Iban { get; init; }
        public required int MerchantId { get; init; }
        public required string Name { get; init; }
        public required string TransferReference { get; init; }
        public required decimal TransferAmount { get; init; }
    }
}
