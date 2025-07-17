using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;

namespace Quivi.Guests.Api.Dtos
{
    public class Fee
    {
        public FeeType Type { get; init; }
        public ChargeMethod PaymentMethod { get; init; }
        public decimal Value { get; init; }
        public FeeUnit Unit { get; init; }
    }
}