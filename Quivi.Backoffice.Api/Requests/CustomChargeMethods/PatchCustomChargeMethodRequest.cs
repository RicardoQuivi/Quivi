using Quivi.Infrastructure.Apis;

namespace Quivi.Backoffice.Api.Requests.CustomChargeMethods
{
    public class PatchCustomChargeMethodRequest : ARequest
    {
        public string? Name { get; init; }
        public Optional<string> LogoUrl { get; init; }
    }
}
