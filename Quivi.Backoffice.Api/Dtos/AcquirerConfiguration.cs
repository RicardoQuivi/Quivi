using Quivi.Domain.Entities.Charges;

namespace Quivi.Backoffice.Api.Dtos
{
    public class AcquirerConfiguration
    {
        public required string Id { get; init; }
        public ChargeMethod Method { get; init; }
        public ChargePartner Partner { get; init; }
        public bool IsActive { get; init; }
        public required IReadOnlyDictionary<ChargePartner, IReadOnlyDictionary<ChargeMethod, object>> Settings { get; init; }
    }
}