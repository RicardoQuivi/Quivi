namespace Quivi.Infrastructure.Abstractions.Pos.Exceptions
{
    public enum SyncingState
    {
        Syncing = 0,
        Success = 1,

        UnknownFailure = -1,
        QrCodeIsInactive = -2,
        SessionDoesNotExist = -3,
        InvalidPoSConfiguration = -4,
        InvalidAmount = -5,
        SessionAlreadyClosed = -6,
        SessionWasAlreadyPaid = -7,
        PoSUnavailable = -8,
        TaxRequirements = -9,
    }

    public class PosPaymentSyncingException : PosException
    {
        public SyncingState State { get; }

        public PosPaymentSyncingException(SyncingState state, Exception exception) : base($"Synchronization issue {state} was found due to exception: {exception.Message}", exception)
        {
            State = state;
        }

        public PosPaymentSyncingException(SyncingState state, string message) : base($"Synchronization issue {state} was found due to exception: {message}")
        {
            State = state;
        }
    }
}
