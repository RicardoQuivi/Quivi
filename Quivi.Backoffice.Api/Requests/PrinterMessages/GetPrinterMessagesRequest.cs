namespace Quivi.Backoffice.Api.Requests.PrinterMessages
{
    public class GetPrinterMessagesRequest : APagedRequest
    {
        public required string PrinterId { get; init; }
    }
}