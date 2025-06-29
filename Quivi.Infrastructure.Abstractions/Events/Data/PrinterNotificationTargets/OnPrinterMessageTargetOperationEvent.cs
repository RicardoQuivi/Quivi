namespace Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationTargets
{
    public record OnPrinterMessageTargetOperationEvent : IEvent
    {
        public EntityOperation Operation { get; init; }
        public int PrinterNotificationMessageId { get; init; }
        public int PrinterNotificationsContactId { get; init; }
        public int MerchantId { get; init; }
    }
}