namespace Quivi.Printer.Contracts
{
    public class PrintMessage
    {
        public required string Id { get; init; }
        public required string MerchantId { get; init; }
        public MessageContentType ContentType { get; init; }
        public required string Content { get; init; }
        public required IEnumerable<PrintMessageTarget> Targets { get; init; }
        public bool EnqueueIfOffline { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
    }

    public class PrintMessageTarget
    {
        public required string Id { get; init; }
        public required string Address { get; init; }
    }
}
