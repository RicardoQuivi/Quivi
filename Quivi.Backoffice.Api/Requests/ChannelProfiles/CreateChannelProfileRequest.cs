using Quivi.Domain.Entities.Pos;

namespace Quivi.Backoffice.Api.Requests.ChannelProfiles
{
    public class CreateChannelProfileRequest
    {
        public required string Name { get; init; } = string.Empty;
        public decimal MinimumPrePaidOrderAmount { get; init; }
        public ChannelFeature Features { get; init; }
        public TimeSpan? SendToPreparationTimer { get; init; }
        public required string PosIntegrationId { get; init; } = string.Empty;
    }
}
