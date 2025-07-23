namespace Quivi.SignalR.Dtos
{
    public class OnBackgroundJobUpdated
    {
        public required string Id { get; init; }
        public required string MerchantId { get; init; }
    }
}