using Quivi.Domain.Entities.Pos;

namespace Quivi.Backoffice.Api.Dtos
{
    public class ChannelProfile
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public decimal MinimumPrePaidOrderAmount { get; init; }
        public TimeSpan? SendToPreparationTimer { get; init; }
        public ChannelFeature Features { get; init; }
        public required string PosIntegrationId { get; init; }
    }
}
