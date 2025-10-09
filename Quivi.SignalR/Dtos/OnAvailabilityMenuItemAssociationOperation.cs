using Quivi.Infrastructure.Abstractions.Events.Data;

namespace Quivi.SignalR.Dtos
{
    public class OnAvailabilityMenuItemAssociationOperation
    {
        public required EntityOperation Operation { get; init; }
        public required string MerchantId { get; init; }
        public required string AvailabilityId { get; init; }
        public required string MenuItemId { get; init; }
    }
}