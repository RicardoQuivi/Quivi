using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Entities.Notifications;

namespace Quivi.Domain.Entities.Pos
{
    public class PrinterWorker : IDeletableEntity
    {
        public int Id { get; set; }

        public required string Identifier { get; set; }
        public string? Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public ICollection<PrinterNotificationsContact>? PrinterNotificationsContacts { get; set; }
        #endregion
    }
}