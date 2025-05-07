namespace Quivi.Infrastructure.Pos.Facturalusa.Models.External
{
    public class InvoiceCancellation : CreditNote 
    {
        public string? Reason { get; set; }
        public string? UrlFile { get; set; }
    }
}
