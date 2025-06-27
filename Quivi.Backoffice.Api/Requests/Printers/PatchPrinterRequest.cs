using Quivi.Infrastructure.Apis;

namespace Quivi.Backoffice.Api.Requests.Printers
{
    public class PatchPrinterRequest : ARequest
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? PrinterWorkerId { get; set; }
        public Optional<string> LocationId { get; set; }
        public IEnumerable<Dtos.NotificationType>? Notifications { get; init; }
    }
}