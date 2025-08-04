using Paybyrd.Api.Requests;
using Paybyrd.Api.Responses;

namespace Paybyrd.Api
{
    public interface IPaybyrdApi
    {
        Task<CreatePaymentResponse> CreatePayment(string apiKey, CreatePaymentRequest request);
        Task<CapturePaymentTransactionResponse> CapturePayment(string apiKey, CapturePaymentTransactionRequest request);
        Task<GetPaymentResponse> GetPayment(string apiKey, GetPaymentRequest request);
        Task<RefundPaymentResponse> RefundPayment(string apiKey, RefundPaymentRequest request);
    }
}