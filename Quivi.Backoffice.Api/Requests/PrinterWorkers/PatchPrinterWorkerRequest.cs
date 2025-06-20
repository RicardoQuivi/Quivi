using Quivi.Infrastructure.Apis;

namespace Quivi.Backoffice.Api.Requests.PrinterWorkers
{
    public class PatchPrinterWorkerRequest : ARequest
    {
        public Optional<string> Name { get; set; }
    }
}