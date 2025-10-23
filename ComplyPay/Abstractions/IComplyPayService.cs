using ComplyPay.Dtos.Requests;
using ComplyPay.Dtos.Responses;

namespace ComplyPay.Abstractions
{
    public interface IComplyPayService
    {
        Task<GetVendorAccountsResponse> GetVendorAccounts(GetVendorAccountsRequest request);
        Task<CreatePaymentResponse> CreatePayment(CreatePaymentRequest request);
        Task<PayoutAllResponse> PayoutAll();
        Task<GetWalletBallanceResponse> GetWalletBallance(GetWalletBallanceRequest request);
    }
}