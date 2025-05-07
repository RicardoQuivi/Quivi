namespace Quivi.Backoffice.Api.Requests.PrinterWorkers
{
    public class GetPrinterWorkersRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}