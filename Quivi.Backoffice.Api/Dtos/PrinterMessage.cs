using Quivi.Domain.Entities.Notifications;

namespace Quivi.Backoffice.Api.Dtos
{
    public enum PrinterMessageStatus
    {
        Unreachable = -3,
        TimedOut = -2,
        Failed = -1,
        Created = 0,
        Processing = 1,
        Success = 2,
    }

    public class PrinterMessage
    {
        public required string MessageId { get; init; }
        public required string PrinterId { get; init; }
        public NotificationMessageType Type { get; init; }
        public required IReadOnlyDictionary<PrinterMessageStatus, DateTimeOffset> Statuses { get; init; }
    }
}