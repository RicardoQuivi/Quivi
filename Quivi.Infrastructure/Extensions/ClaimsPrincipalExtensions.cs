using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Claims;
using Quivi.Infrastructure.Roles;
using System.Security.Claims;
using System.Security.Principal;

namespace Quivi.Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string UserId(this IPrincipal principal) => principal.GetClaim(QuiviClaims.UserId)?.Value ?? string.Empty;
        public static int UserId(this IPrincipal principal, IIdConverter idConverter) => idConverter.FromPublicId(principal.UserId());

        public static string? MerchantId(this IPrincipal principal) => principal.GetClaim(QuiviClaims.MerchantId)?.Value;
        public static int? MerchantId(this IPrincipal principal, IIdConverter idConverter)
        {
            var id = principal.MerchantId();
            return id == null ? null : idConverter.FromPublicId(id);
        }

        public static string? SubMerchantId(this IPrincipal principal) => principal.GetClaim(QuiviClaims.SubMerchantId)?.Value;
        public static int? SubMerchantId(this IPrincipal principal, IIdConverter idConverter)
        {
            var id = principal.SubMerchantId();
            return id == null ? null : idConverter.FromPublicId(id);
        }

        public static string? EmployeeId(this IPrincipal principal) => principal.GetClaim(QuiviClaims.EmployeeId)?.Value;
        public static int? EmployeeId(this IPrincipal principal, IIdConverter idConverter)
        {
            var id = principal.EmployeeId();
            return id == null ? null : idConverter.FromPublicId(id);
        }

        public static bool IsSuperAdmin(this IPrincipal principal) => principal.IsInRole(QuiviRoles.SuperAdmin);
        public static bool IsAdmin(this IPrincipal principal) => principal.IsSuperAdmin() || principal.IsInRole(QuiviRoles.Admin);

        public static Claim? GetClaim(this IPrincipal principal, string claim)
        {
            var claimsPrincipal = principal as ClaimsPrincipal;
            if (claimsPrincipal == null)
                throw new Exception($"{nameof(IPrincipal)} is not a {nameof(ClaimsPrincipal)}");

            return claimsPrincipal.FindFirst(claim);
        }
    }
}
