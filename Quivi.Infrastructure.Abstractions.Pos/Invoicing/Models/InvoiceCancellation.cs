namespace Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models
{
    public class InvoiceCancellation : CreditNote
    {
        public required string Reason { get; set; }
    }
}