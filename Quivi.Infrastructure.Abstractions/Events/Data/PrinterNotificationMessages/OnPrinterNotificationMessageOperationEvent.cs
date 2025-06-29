namespace Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationMessages
{
    public record OnPrinterNotificationMessageOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}