namespace Quivi.Application.Commands.Settlements
{
    public interface IUpdatableSettlementDetail : IUpdatableEntity
    {
        int JournalId { get; }
        decimal Amount { get; set; }
        decimal IncludedTip { get; set; }
        string MerchantIban { get; set; }
        decimal MerchantVatRate { get; set; }
        decimal TransactionFee { get; set; }
        decimal FeeAmount { get; set; }
        decimal VatAmount { get; set; }
        decimal NetAmount { get; set; }
        decimal IncludedNetTip { get; set; }
        int ParentMerchantId { get; set; }
        int MerchantId { get; set; }
    }
}