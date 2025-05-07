namespace Quivi.Infrastructure.Pos.Facturalusa.Models.External
{
    public class InvoiceReceipt : ADocument
    {
        public required Customer Customer { get; set; }
    }
}
