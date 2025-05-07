namespace Quivi.Domain.Entities.Pos
{
    [Flags]
    public enum ChannelFeature
    {
        None = 0,

        /// <summary>
        /// QR Code supports session based consumption. If set,
        /// the QR Code can have consumptions associated with it (via POS or App).
        /// </summary>
        AllowsSessions = 1 << 0,

        /// <summary>
        /// QR Code supports Order And Pay. If set, a client can place an order via App.
        /// </summary>
        AllowsOrderAndPay = 1 << 1,

        /// <summary>
        /// QR Code supports Pay At The table. If set, a client can pay via App. 
        /// </summary>
        AllowsPayAtTheTable = 1 << 2,

        /// <summary>
        /// QR Code requires an email to be provided when placing an Order and Pay.
        /// </summary>
        RequiresEmailForOrderAndPay = 1 << 3,

        /// <summary>
        /// QR Code supports Post Payment Ordering. If set, a client can add order items to a current session.
        /// </summary>
        AllowsPostPaymentOrdering = 1 << 4,

        /// <summary>
        /// If set, after an Order and Pay, the final screen will show a tracking widget.
        /// </summary>
        OrderAndPayWithTracking = 1 << 5,

        /// <summary>
        /// If set allows scheduling when performing an Order And Pay.
        /// </summary>
        OrderScheduling = 1 << 6,

        /// <summary>
        /// If set defines the QR Code is a physical kiosk,
        /// </summary>
        PhysicalKiosk = 1 << 7,

        /// <summary>
        /// If set defines a Post Paid Order to be automatically approved and go onto the next step
        /// </summary>
        PostPaidOrderingAutoApproval = 1 << 8,

        /// <summary>
        /// If set defines a Pre Paid Order to be automatically approved and go onto the next step
        /// </summary>
        PrePaidOrderingAutoApproval = 1 << 9,

        /// <summary>
        /// If set defines a Post Paid Order to be automatically completed when it reaches the state "Processing"
        /// </summary>
        PostPaidOrderingAutoComplete = 1 << 10,

        /// <summary>
        /// If set defines a Pre Paid Order to be automatically completed when it reaches the state "Processing"
        /// </summary>
        PrePaidOrderingAutoComplete = 1 << 11,

        /// <summary>
        /// If set defines a QR Code allow users to send random payments
        /// </summary>
        AllowsFreePayments = 1 << 12,

        /// <summary>
        /// If set, free payments are considered tip.
        /// </summary>
        FreePaymentsAsTipOnly = 1 << 13,

        /// <summary>
        /// QR Code is Take Away Only. If set, This QR Code does not allow sessions (and thus Pay at the table payments)
        /// and an email is required when creating an Order And Pay.
        /// </summary>
        IsTakeAwayOnly = (AllowsOrderAndPay | RequiresEmailForOrderAndPay) & (~AllowsPayAtTheTable) & (~AllowsSessions),
    }
}
