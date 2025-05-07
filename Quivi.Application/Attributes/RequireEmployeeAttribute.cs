using Quivi.Infrastructure.Claims;

namespace Quivi.Application.Attributes
{
    public class RequireEmployeeAttribute : ClaimAuthorizeAttribute
    {
        public RequireEmployeeAttribute() : base(QuiviClaims.EmployeeId)
        {
        }
    }
}