﻿namespace Quivi.Domain.Entities.Notifications
{
    [Flags]
    public enum NotificationMessageType : long
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Test.
        /// </summary>
        Test = 1 << 0,

        /// <summary>
        /// Failed charge notification.
        /// </summary>
        FailedCharge = 1 << 1,

        /// <summary>
        /// Expired charge notification.
        /// </summary>
        ExpiredCharge = 1 << 2,

        /// <summary>
        /// PoS is Offline.
        /// </summary>
        PosOffline = 1 << 3,

        /// <summary>
        /// PoS synchronization failure.
        /// </summary>
        PosSyncFailure = 1 << 4,

        /// <summary>
        /// PoS payment synchronization failure.
        /// </summary>
        PosPaymentSyncFailure = 1 << 5,

        /// <summary>
        /// Payment arrived.
        /// </summary>
        CompletedCharge = 1 << 6,

        /// <summary>
        /// Order arrived.
        /// </summary>
        NewOrder = 1 << 7,

        /// <summary>
        /// Consumer sent a review
        /// </summary>
        NewReview = 1 << 8,

        /// <summary>
        /// Consumer bill
        /// </summary>
        ConsumerBill = 1 << 9,

        /// <summary>
        /// Consumer invoice generated
        /// </summary>
        NewConsumerInvoice = 1 << 10,

        /// <summary>
        /// New order sent to kitchen
        /// </summary>
        NewPreparationRequest = 1 << 11,

        /// <summary>
        /// Charge synced with POS
        /// </summary>
        ChargeSynced = 1 << 12,

        /// <summary>
        /// Trigger Open Cash Drawer
        /// </summary>
        OpenCashDrawer = 1 << 13,

        /// <summary>
        /// Trigger an End of Day Closing
        /// </summary>
        EndOfDayClosing = 1 << 14,
    }
}
