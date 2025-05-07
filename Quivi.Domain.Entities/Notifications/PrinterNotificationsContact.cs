using Quivi.Domain.Entities.Pos;

namespace Quivi.Domain.Entities.Notifications
{
    public class PrinterNotificationsContact : IDeletableEntity
    {
        public int Id => NotificationsContactId;

        public required string Name { get; set; }
        public required string Address { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int NotificationsContactId { get; set; }
        public NotificationsContact? BaseNotificationsContact { get; set; }

        public int PrinterWorkerId { get; set; }
        public PrinterWorker? PrinterWorker { get; set; }

        public int? LocationId { get; set; }
        public Location? Location { get; set; }
        #endregion
    }
}