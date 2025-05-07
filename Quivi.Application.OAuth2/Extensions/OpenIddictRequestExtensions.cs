using OpenIddict.Abstractions;

namespace Quivi.Application.OAuth2.Extensions
{
    public static class OpenIddictRequestExtensions
    {
        public static bool IsEmployeeGrantType(this OpenIddictRequest request)
        {
            return string.Equals(request.GrantType, CustomGrantTypes.Employee, StringComparison.Ordinal);
        }

        public static bool IsTokenExchangeGrantType(this OpenIddictRequest request)
        {
            return string.Equals(request.GrantType, CustomGrantTypes.TokenExchange, StringComparison.Ordinal);
        }

        public static string? SubjectToken(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter("subject_token");
            return parameter?.Value?.ToString();
        }

        public static string? SubjectType(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter("subject_token_type");
            return parameter?.Value?.ToString();
        }

        public static string? Username(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter("username");
            return parameter?.Value?.ToString();
        }

        public static string? Password(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter("password");
            return parameter?.Value?.ToString();
        }

        public static string? RefreshToken(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter("refresh_token");
            return parameter?.Value?.ToString();
        }

        public static string? MerchantId(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter("merchant_id");
            return parameter?.Value?.ToString();
        }
    }
}
