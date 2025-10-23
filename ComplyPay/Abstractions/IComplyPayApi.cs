using ComplyPay.Dtos.Requests;
using ComplyPay.Dtos.Responses;

namespace ComplyPay.Abstractions
{
    public interface IComplyPayApi
    {
        Task<AuthenticateJwtResponse> AuthenticateJwt(AuthenticateJwtRequest request);
        Task<JwtRefreshResponse> JwtRefresh(JwtRefreshRequest request);

        Task<GetVendorAccountsResponse> GetVendorAccounts(string token, GetVendorAccountsRequest request);

        Task<CreatePaymentResponse> CreatePayment(string token, CreatePaymentRequest request);

        Task<PayoutAllResponse> PayoutAll(string token);

        Task<GetWalletBallanceResponse> GetWalletBallance(string token, GetWalletBallanceRequest request);
    }
}