using Quivi.Infrastructure.Abstractions.Events.Data;

namespace Quivi.SignalR.Dtos
{
    public class OnOrderAdditionalInfoOperation
    {
        public EntityOperation Operation { get; init; }
        public required string OrderConfigurableFieldId { get; init; }
        public required string OrderId { get; init; }
        public required string MerchantId { get; init; }
    }
}