using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Notifications
{
    public class PrinterNotificationMessage : IEntity
    {
        public int Id { get; set; }
        public NotificationMessageType MessageType { get; set; }
        public PrinterMessageContentType ContentType { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public ICollection<PrinterMessageTarget>? PrinterMessageTargets { get; set; }

        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }
        #endregion
    }
}