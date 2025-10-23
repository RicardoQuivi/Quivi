using ComplyPay.Abstractions;
using ComplyPay.Dtos.Requests;
using ComplyPay.Dtos.Responses;

namespace ComplyPay
{
    public class ComplyPayService : IComplyPayService
    {
        private record ApiTokens
        {
            public required string AccessToken { get; init; }
            public required string RefreshToken { get; init; }
        }

        private readonly IComplyPayApi api;
        private readonly string email;
        private readonly string password;

        private ApiTokens? Tokens { get; set; }

        public ComplyPayService(IComplyPayApi api, string email, string password)
        {
            this.api = api;
            this.email = email;
            this.password = password;
        }

        private async Task<T> AuthorizedRequest<T>(Func<string, Task<T>> func)
        {
            try
            {
                if (Tokens == null)
                    Tokens = await Authenticate();
                return await func(Tokens.AccessToken);
            }
            catch (UnauthorizedAccessException)
            {
                Tokens = await Authenticate();
                return await func(Tokens.AccessToken);
            }
        }

        private async Task<ApiTokens> Authenticate()
        {
            if (Tokens?.RefreshToken == null)
            {
                var authenticateResponse = await api.AuthenticateJwt(new AuthenticateJwtRequest
                {
                    Email = email,
                    Password = password,
                });
                return new ApiTokens
                {
                    AccessToken = authenticateResponse.Token,
                    RefreshToken = authenticateResponse.RefreshToken,
                };
            }

            var jwtRefreshResponse = await api.JwtRefresh(new JwtRefreshRequest
            {
                RefreshToken = Tokens.RefreshToken,
            });

            return new ApiTokens
            {
                AccessToken = jwtRefreshResponse.Token,
                RefreshToken = jwtRefreshResponse.RefreshToken,
            };
        }

        public Task<CreatePaymentResponse> CreatePayment(CreatePaymentRequest request) => AuthorizedRequest((accessToken) => api.CreatePayment(accessToken, request));

        public Task<GetVendorAccountsResponse> GetVendorAccounts(GetVendorAccountsRequest request) => AuthorizedRequest((accessToken) => api.GetVendorAccounts(accessToken, request));

        public Task<PayoutAllResponse> PayoutAll() => AuthorizedRequest(api.PayoutAll);

        public Task<GetWalletBallanceResponse> GetWalletBallance(GetWalletBallanceRequest request) => AuthorizedRequest((accessToken) => api.GetWalletBallance(accessToken, request));
    }
}