namespace Quivi.Backoffice.Api.Requests.Printers
{
    public class CreatePrinterRequest : ARequest
    {
        public required string Name { get; init; }
        public required string Address { get; init; }
        public required string PrinterWorkerId { get; init; }
        public string? LocationId { get; init; }
        public required IEnumerable<Dtos.NotificationType> Notifications { get; init; } = [];
    }
}
