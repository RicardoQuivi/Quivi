namespace Quivi.Backoffice.Api.Requests.PrinterWorkers
{
    public class CreatePrinterWorkerRequest : ARequest
    {
        public required string Identifier { get; init; }
        public required string Name { get; init; }
    }
}