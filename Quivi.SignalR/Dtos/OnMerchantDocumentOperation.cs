using Quivi.Infrastructure.Abstractions.Events.Data;

namespace Quivi.SignalR.Dtos
{
    public class OnMerchantDocumentOperation
    {
        public required EntityOperation Operation { get; init; }
        public required string MerchantId { get; init; }
        public required string Id { get; init; }
        public string? PosChargeId { get; init; }
    }
}