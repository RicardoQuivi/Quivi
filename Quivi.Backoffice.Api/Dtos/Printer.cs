namespace Quivi.Backoffice.Api.Dtos
{
    public class Printer
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Address { get; init; }
        public required string PrinterWorkerId { get; init; }
        public string? LocationId { get; init; }
        public required IEnumerable<NotificationType> Notifications { get; init; }
    }
}