namespace Quivi.Domain.Entities.Pos
{
    public enum InvoiceDocumentType
    {
        OrderInvoice = 0,
        SurchargeInvoice = 1,
        CreditNote = 2,
        MerchantMonthlyInvoice = 3,
        InvoiceCancellation = 4,
    }
}