namespace Quivi.Guests.Api.Dtos.Requests.Orders
{
    public class GetOrdersRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; set; }
        public IEnumerable<string>? ChargeIds { get; set; }
        public IEnumerable<string>? ChannelIds { get; set; }
        public string? SessionId { get; set; }
    }
}