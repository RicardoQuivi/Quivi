namespace Quivi.Guests.Api.Dtos
{
    public class OrderChangeLog
    {
        public OrderState State { get; init; }
        public string? Note { get; init; }
        public DateTimeOffset LastModified { get; init; }
    }
}