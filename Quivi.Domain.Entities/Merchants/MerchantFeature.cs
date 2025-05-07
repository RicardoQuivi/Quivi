namespace Quivi.Domain.Entities.Merchants
{
    [Flags]
    public enum MerchantFeature
    {
        None = 0,
        FreePayment = 1 << 0,
        ItemSelectionPayment = 1 << 1,
        SplitBillPayment = 1 << 2,
        EnforceTip = 1 << 3,
        HidePaymentNotes = 1 << 5,
        ShowFeaturedPromo = 1 << 8,
        DisallowNotMyBill = 1 << 14,
        AllowSurchargeFeeForPT = 1 << 15,
    }
}
