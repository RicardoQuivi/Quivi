namespace Quivi.Backoffice.Api.Dtos
{
    public class SettlementDetail
    {
        public required string Id { get; init; }
        public required string SettlementId { get; init; }

        public DateTimeOffset Date { get; init; }

        public required string ParentMerchantId { get; init; }
        public required string MerchantId { get; init; }

        public decimal GrossAmount { get; init; }
        public decimal GrossTip { get; init; }
        public decimal GrossTotal { get; init; }

        public decimal NetAmount { get; init; }
        public decimal NetTip { get; init; }
        public decimal NetTotal { get; init; }
    }
}