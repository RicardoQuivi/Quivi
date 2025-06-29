namespace Quivi.Domain.Entities.Notifications
{
    public enum AuditStatus
    {
        Failed = -3,
        TimedOut = -2,
        Unreachable = -1,
        Pending = 0,
        Success = 1,
    }

    public class PrinterMessageTarget : IEntity
    {
        public DateTime CreatedDate { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public AuditStatus Status { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int PrinterNotificationMessageId { get; set; }
        public PrinterNotificationMessage? PrinterNotificationMessage { get; set; }

        public int PrinterNotificationsContactId { get; set; }
        public PrinterNotificationsContact? PrinterNotificationsContact { get; set; }
        #endregion
    }
}