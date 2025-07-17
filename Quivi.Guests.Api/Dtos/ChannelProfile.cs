using Quivi.Domain.Entities.Pos;

namespace Quivi.Guests.Api.Dtos
{
    public class ChannelProfile
    {
        public required string Id { get; init; }
        public required string PosIntegrationId { get; init; }
        public required string Name { get; init; }
        public ChannelFeature Features { get; init; }
        public decimal PrePaidOrderingMinimumAmount { get; init; }
    }
}