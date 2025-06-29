namespace Quivi.Backoffice.Api.Requests.Printers
{
    public class TestPrinterRequest
    {
        public string? Text { get; init; }
        public bool PingOnly { get; init; }
    }
}