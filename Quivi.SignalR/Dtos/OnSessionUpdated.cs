namespace Quivi.SignalR.Dtos
{
    public class OnSessionUpdated
    {
        public required string MerchantId { get; init; }
        public required string ChannelId { get; init; }
        public required string Id { get; init; }
    }
}