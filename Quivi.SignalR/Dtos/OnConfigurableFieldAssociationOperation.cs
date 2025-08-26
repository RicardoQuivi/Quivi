using Quivi.Infrastructure.Abstractions.Events.Data;

namespace Quivi.SignalR.Dtos
{
    public class OnConfigurableFieldAssociationOperation
    {
        public required string MerchantId { get; init; }
        public required EntityOperation Operation { get; init; }
        public required string ChannelProfileId { get; init; }
        public required string ConfigurableFieldId { get; init; }
    }
}