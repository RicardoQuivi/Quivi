namespace Quivi.Infrastructure.Abstractions.Events.Data.PrinterNotificationsContacts
{
    public record OnPrinterNotificationsContactOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}