
namespace Quivi.Domain.Entities.Notifications
{
    public class PushNotificationsContact : IDeletableEntity
    {
        public int Id => NotificationsContactId;

        public required string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int NotificationsContactId { get; set; }
        public required NotificationsContact BaseNotificationsContact { get; set; }

        public int PushDeviceId { get; set; }
        public required PushNotificationDevice PushDevice { get; set; }
        #endregion
    }
}
