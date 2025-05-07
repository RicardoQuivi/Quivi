using Quivi.Domain.Entities.Pos;

namespace Quivi.Domain.Entities.Notifications
{
    public class EmployeeNotificationsContact : IDeletableEntity
    {
        public int Id => NotificationsContactId;

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int NotificationsContactId { get; set; }
        public required NotificationsContact BaseNotificationsContact { get; set; }

        public int EmployeeId { get; set; }
        public required Employee Employee { get; set; }
        #endregion
    }
}