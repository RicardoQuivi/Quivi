namespace Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models
{
    public class CreditNote : ADocument
    {
        public required string RelatedDocumentId { get; set; }
        public required Customer Customer { get; set; }
        public required string PaymentMethodCode { get; init; }
    }
}