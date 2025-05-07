using Quivi.Domain.Entities.Pos;

namespace Quivi.Pos.Api.Dtos
{
    public class ChannelProfile
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public ChannelFeature Features { get; init; }
        public required string PosIntegrationId { get; init; }
    }
}
