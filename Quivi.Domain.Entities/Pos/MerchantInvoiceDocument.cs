using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class MerchantInvoiceDocument : IEntity
    {
        public int Id { get; set; }

        public string? DocumentId { get; set; }
        public string? DocumentReference { get; set; }
        public InvoiceDocumentType DocumentType { get; set; }
        public string? Path { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public int? ChargeId { get; set; }
        public Charge? Charge { get; set; }
        #endregion
    }
}