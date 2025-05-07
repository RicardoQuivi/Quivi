using Quivi.Infrastructure.Claims;

namespace Quivi.Application.Attributes
{
    public class RequireMerchantAttribute : ClaimAuthorizeAttribute
    {
        public RequireMerchantAttribute() : base(QuiviClaims.MerchantId)
        {
        }
    }
}