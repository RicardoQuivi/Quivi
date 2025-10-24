using OpenIddict.Abstractions;

namespace Quivi.Application.OAuth2
{
    public static class RequestParameters
    {
        public const string SubjectToken = "subject_token";
        public const string SubjectTokenType = "subject_token_type";
        public const string Username = OpenIddictConstants.Parameters.Username;
        public const string Password = OpenIddictConstants.Parameters.Password;
        public const string RefreshToken = OpenIddictConstants.Parameters.RefreshToken;
        public const string MerchantId = "merchant_id";
    }
}