namespace Quivi.Domain.Entities.Notifications
{
    public class AuditNotification : IEntity
    {
        public int AuditNotificationId { get; set; }

        public NotificationMessageType NotificationType { get; set; }
        public DateTime? LastSentDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int NotificationsContactId { get; set; }
        public required NotificationsContact NotificationsContact { get; set; }
        #endregion
    }
}
