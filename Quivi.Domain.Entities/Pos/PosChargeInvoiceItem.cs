namespace Quivi.Domain.Entities.Pos
{
    public class PosChargeInvoiceItem : IEntity
    {
        public int Id { get; set; }

        public decimal Quantity { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int? ParentPosChargeInvoiceItemId { get; set; }
        public PosChargeInvoiceItem? ParentPosChargeInvoiceItem { get; set; }

        public int PosChargeId { get; set; }
        public PosCharge? PosCharge { get; set; }

        public int OrderMenuItemId { get; set; }
        public OrderMenuItem? OrderMenuItem { get; set; }

        public ICollection<PosChargeInvoiceItem>? ChildrenPosChargeInvoiceItems { get; set; }
        #endregion
    }
}