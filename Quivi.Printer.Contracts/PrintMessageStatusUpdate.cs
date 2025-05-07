namespace Quivi.Printer.Contracts
{
    public class PrintMessageStatusUpdate
    {
        public required string MerchantId { get; init; }
        public required string MessageId { get; init; }
        public required string PrinterId { get; init; }
        public required PrintStatus Status { get; init; }
        public required DateTime UtcDate { get; init; }
    }
}
