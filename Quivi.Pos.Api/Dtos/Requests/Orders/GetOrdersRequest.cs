using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories.Data;

namespace Quivi.Pos.Api.Dtos.Requests.Orders
{
    public class GetOrdersRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public IEnumerable<string>? ChannelIds { get; init; }
        public IEnumerable<string>? SessionIds { get; init; }
        public IEnumerable<OrderState>? States { get; init; }
        public SortDirection? SortDirection { get; init; }
    }
}