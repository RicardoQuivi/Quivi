using Quivi.Domain.Entities.Pos;

namespace Quivi.Pos.Api.Dtos
{
    public class Order
    {
        public required string Id { get; init; }
        public required string SequenceNumber { get; init; }
        public required string ChannelId { get; init; }
        public string? EmployeeId { get; init; }
        public required IEnumerable<SessionItem> Items { get; init; }
        public required IEnumerable<OrderFieldValue> Fields { get; init; }
        public OrderState State { get; init; }
        public OrderOrigin OrderOrigin { get; init; }
        public bool IsTakeAway { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
        public DateTimeOffset LastModified { get; init; }
    }

    public class OrderFieldValue
    {
        public required string Id { get; init; }
        public required string Value { get; init; }
    }
}