namespace Paybyrd.Api.Models
{
    public enum PaymentStatus
    {
        Created,
        Processing,
        TemporaryFailed,
        Denied,
        Success,
        Canceled,
        Error,
        PendingMerchantAction,
    }
}
