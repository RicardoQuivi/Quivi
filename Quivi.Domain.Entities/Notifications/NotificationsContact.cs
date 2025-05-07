using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Notifications
{
    public class NotificationsContact : IDeletableEntity
    {
        public int Id { get; set; }

        public NotificationMessageType SubscribedNotifications { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public ICollection<AuditNotification>? AuditNotifications { get; set; }
        //TODO: Should this be kept?
        //public WhatsappNotificationsContact WhatsappContact { get; set; }
        //public EmailNotificationsContact EmailContact { get; set; }
        public PushNotificationsContact? PushContact { get; set; }
        public PrinterNotificationsContact? PrinterContact { get; set; }
        public EmployeeNotificationsContact? EmployeeContact { get; set; }
        #endregion
    }
}
