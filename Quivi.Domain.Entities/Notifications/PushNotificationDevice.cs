using Quivi.Domain.Entities.Financing;

namespace Quivi.Domain.Entities.Notifications
{
    public class PushNotificationDevice : IDeletableEntity
    {
        public int PushNotificationDeviceId { get; set; }

        public required string DeviceToken { get; set; }
        //TODO: Can probably delete this field
        public Guid SessionGuid { get; set; }
        public ClientType DeviceType { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int PersonId { get; set; }
        public required Person Person { get; set; }

        public ICollection<PushNotificationsContact>? Contacts { get; set; }
        #endregion
    }
}
