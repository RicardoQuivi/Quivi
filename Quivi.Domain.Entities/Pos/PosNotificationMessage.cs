using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Entities.Notifications;

namespace Quivi.Domain.Entities.Pos
{
    public class PosNotificationMessage : IEntity
    {
        public int Id { get; set; }
        public NotificationMessageType Type { get; set; }
        public required string JsonMessage { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public required Merchant Merchant { get; set; }

        public ICollection<PosNotificationInboxMessage> PosNotificationInboxes { get; set; }
        #endregion
    }
}
