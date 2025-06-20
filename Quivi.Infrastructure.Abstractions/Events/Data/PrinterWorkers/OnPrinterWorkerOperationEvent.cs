namespace Quivi.Infrastructure.Abstractions.Events.Data.PrinterWorkers
{
    public record OnPrinterWorkerOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}