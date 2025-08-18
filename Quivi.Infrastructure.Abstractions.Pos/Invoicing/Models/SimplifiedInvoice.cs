namespace Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models
{
    public class SimplifiedInvoice : ADocument
    {
        public string? CustomerVatNumber { get; init; }
        public required string PaymentMethodCode { get; init; }
    }
}