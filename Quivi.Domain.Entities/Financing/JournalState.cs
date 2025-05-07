namespace Quivi.Domain.Entities.Financing
{
    public enum JournalState
    {
        Requested = 0, // Payment or QR Code is created AKA Pending
        Completed = 1, // Consumer accepted the request to Pay and Quivi also accepted the transaction
        RejectedByUser = 2, // Consumer rejected the request to Pay
        Rejected = 3, // Quivi Server rejected the payment (Insufficient balance, etc)
        Canceled = 4, // POS Canceled the payment while waiting for user acceptance
        Expired = 5,
        ExpiredAuth = 6,
    }
}
