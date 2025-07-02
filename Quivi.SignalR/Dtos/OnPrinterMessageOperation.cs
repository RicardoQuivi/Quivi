using Quivi.Infrastructure.Abstractions.Events.Data;

namespace Quivi.SignalR.Dtos
{
    public class OnPrinterMessageOperation
    {
        public required EntityOperation Operation { get; init; }
        public required string MerchantId { get; init; }
        public required string MessageId { get; init; }
        public required string PrinterId { get; init; }
    }
}