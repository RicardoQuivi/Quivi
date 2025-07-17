using Quivi.Infrastructure.Abstractions.Events.Data;

namespace Quivi.SignalR.Dtos
{
    public class OnPosChargeOperation
    {
        public required EntityOperation Operation { get; init; }
        public required string MerchantId { get; init; }
        public required string ChannelId { get; init; }
        public required string Id { get; init; }
    }
}