namespace Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models
{
    public class InvoiceReceipt : ADocument
    {
        public required string PaymentMethodCode { get; set; }
        public required Customer Customer { get; set; }
    }
}
