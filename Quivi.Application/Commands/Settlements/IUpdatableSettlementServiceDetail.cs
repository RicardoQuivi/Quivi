namespace Quivi.Application.Commands.Settlements
{
    public interface IUpdatableSettlementServiceDetail : IUpdatableEntity
    {
        int JournalId { get; }

        int MerchantServiceId { get; set; }
        int ParentMerchantId { get; set; }
        string MerchantIban { get; set; }
        decimal MerchantVatRate { get; set; }
        int MerchantId { get; set; }
        decimal Amount { get; set; }
        decimal VatAmount { get; set; }
    }
}