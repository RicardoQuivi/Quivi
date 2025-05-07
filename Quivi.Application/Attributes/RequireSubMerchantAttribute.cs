using Quivi.Infrastructure.Claims;

namespace Quivi.Application.Attributes
{
    public class RequireSubMerchantAttribute : ClaimAuthorizeAttribute
    {
        public RequireSubMerchantAttribute() : base(QuiviClaims.SubMerchantId)
        {
        }
    }
}