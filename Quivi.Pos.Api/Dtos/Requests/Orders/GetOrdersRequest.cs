using Quivi.Domain.Entities.Pos;

namespace Quivi.Pos.Api.Dtos.Requests.Orders
{
    public class GetOrdersRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public IEnumerable<string>? SessionIds { get; init; }
        public IEnumerable<OrderState>? States { get; init; }
    }
}