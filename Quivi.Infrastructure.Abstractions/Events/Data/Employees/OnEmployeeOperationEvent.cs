namespace Quivi.Infrastructure.Abstractions.Events.Data.Employees
{
    public record OnEmployeeOperationEvent : IOperationEvent
    {
        public EntityOperation Operation { get; init; }
        public int Id { get; init; }
        public int MerchantId { get; init; }
    }
}
