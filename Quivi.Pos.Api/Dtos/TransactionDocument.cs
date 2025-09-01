namespace Quivi.Pos.Api.Dtos
{
    public enum TransactionDocumentType
    {
        Order = 0,
        Surcharge = 1,
        Cancellation = 2,
        CreditNote = 3,
    }

    public class TransactionDocument
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Url { get; init; }
        public TransactionDocumentType Type { get; init; }
    }
}