namespace Quivi.Guests.Api.Dtos
{
    public class Session
    {
        public required string Id { get; init; }
        public required string ChannelId { get; init; }
        public required IEnumerable<SessionItem> Items { get; init; }
    }
}