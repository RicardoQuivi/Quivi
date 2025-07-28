namespace Quivi.Guests.Api.Dtos
{
    public enum OrderState
    {
        Draft = -2,
        Requested = 0,
        Scheduled = 1,
        Rejected = 2,
        Processing = 3,
        Completed = 4,
    }

    public enum OrderType
    {
        OnSite = 0,
        TakeAway = 1,
    }

    public enum ExtraCostType
    {
        None = 0,
        SurchargeFee = 1,
        Tip = 2,
    }

    public class Order
    {
        public required string Id { get; init; }
        public required string MerchantId { get; init; }
        public required string SequenceNumber { get; init; }
        public required string ChannelId { get; init; }
        public OrderState State { get; init; }
        public OrderType Type { get; init; }
        public required IEnumerable<OrderItem> Items { get; init; }
        public DateTimeOffset? ScheduledTo { get; init; }
        public DateTimeOffset LastModified { get; init; }
        public required IEnumerable<OrderExtraCost> ExtraCosts { get; init; }
        public required IEnumerable<OrderChangeLog> Changes { get; init; }
        public required IEnumerable<OrderFieldValue> Fields { get; init; }
    }
}