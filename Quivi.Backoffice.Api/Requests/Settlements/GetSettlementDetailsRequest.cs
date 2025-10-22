using Quivi.Domain.Entities.Charges;

namespace Quivi.Backoffice.Api.Requests.Settlements
{
    public class GetSettlementDetailsRequest : APagedRequest
    {
        public ChargeMethod? ChargeMethod { get; init; }
    }
}