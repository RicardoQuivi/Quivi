namespace Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models
{
    public class InvoiceCancellation : CreditNote
    {
        public string? Reason { get; set; }
    }
}