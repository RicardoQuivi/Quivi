namespace Quivi.SignalR.Dtos.Guests
{
    public class OnMenuItemAvailabilityChanged
    {
        public required string MerchantId { get; init; }
        public required string ChannelProfileId { get; init; }
        public required string Id { get; init; }
    }
}