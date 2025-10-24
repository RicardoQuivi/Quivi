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
            var parameter = request.GetParameter(RequestParameters.SubjectToken);
            return parameter?.Value?.ToString();
        }

        public static string? SubjectType(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter(RequestParameters.SubjectTokenType);
            return parameter?.Value?.ToString();
        }

        public static string? Username(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter(RequestParameters.Username);
            return parameter?.Value?.ToString();
        }

        public static string? Password(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter(RequestParameters.Password);
            return parameter?.Value?.ToString();
        }

        public static string? RefreshToken(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter(RequestParameters.RefreshToken);
            return parameter?.Value?.ToString();
        }

        public static string? MerchantId(this OpenIddictRequest request)
        {
            var parameter = request.GetParameter(RequestParameters.MerchantId);
            return parameter?.Value?.ToString();
        }
    }
}