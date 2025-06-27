namespace Quivi.Backoffice.Api.Dtos
{
    public enum NotificationType
    {
        FailedCharge,
        ExpiredCharge,
        PosOffline,
        PosSyncFailure,
        PosPaymentSyncFailure,
        CompletedCharge,
        NewOrder,
        NewReview,
        NewConsumerBill,
        NewConsumerInvoice,
        NewPreparationRequest,
        ChargeSynced,
        OpenCashDrawer,
        EndOfDayClosing,
    }
}