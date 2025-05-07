namespace Quivi.Infrastructure.Pos.Facturalusa.Models.External
{
    public class CreditNote : ADocument
    {
        public required string RelatedDocumentId { get; set; }
        public required Customer Customer { get; set; }
    }
}