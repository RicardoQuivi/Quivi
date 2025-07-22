using Quivi.Domain.Entities.Pos;

namespace Quivi.Guests.Api.Dtos
{
    public class TransactionInvoice
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string TransactionId { get; init; }
        public required string DownloadUrl { get; init; }
        public InvoiceDocumentType Type { get; init; }
    }
}