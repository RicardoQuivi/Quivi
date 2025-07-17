namespace Quivi.Backoffice.Api.Requests.PrinterMessages
{
    public class CreatePrinterMessageRequest
    {
        public required string PrinterId { get; init; }
        public required string Text { get; init; }
        public bool PingOnly { get; init; }
        public DateTime? Timestamp { get; init; }
    }
}