namespace Paybyrd.Api.Models
{
    public enum TransactionStatus
    {
        Created,
        Processing,
        TemporaryFailed,
        Denied,
        Success,
        Canceled,
        Error,
    }
}