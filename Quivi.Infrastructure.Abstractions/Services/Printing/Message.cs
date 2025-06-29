namespace Quivi.Infrastructure.Abstractions.Services.Printing
{
    public class Target
    {
        public int PrinterNotificationsContactId { get; init; }
        public required string Address { get; init; }
    }

    public class Message
    {
        public int PrinterNotificationMessageId { get; init; }
        public int MerchantId { get; init; }
        public required string Content { get; init; }
        public required IEnumerable<Target> Targets { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
    }
}