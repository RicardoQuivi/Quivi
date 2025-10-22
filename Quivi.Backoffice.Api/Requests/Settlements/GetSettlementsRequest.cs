using Quivi.Domain.Entities.Charges;

namespace Quivi.Backoffice.Api.Requests.Settlements
{
    public class GetSettlementsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public ChargeMethod? ChargeMethod { get; init; }
    }
}