using Quivi.Domain.Entities.Charges;

namespace Quivi.Guests.Api.Dtos
{
    public class PaymentMethod
    {
        public required string Id { get; init; }
        public required ChargeMethod Method { get; init; }
    }
}