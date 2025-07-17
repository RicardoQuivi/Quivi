namespace Quivi.Domain.Entities.Pos
{
    public class OrderMenuItem : IEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public PriceType PriceType { get; set; }
        public decimal VatRate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }


        #region Relationships
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public int? ParentOrderMenuItemId { get; set; }
        public OrderMenuItem? ParentOrderMenuItem { get; set; }
        public ICollection<OrderMenuItem>? Modifiers { get; set; }

        public int? MenuItemModifierGroupId { get; set; }
        public ItemsModifierGroup? MenuItemModifierGroup { get; set; }

        public ICollection<PosChargeInvoiceItem>? PosChargeInvoiceItems { get; set; }
        #endregion
    }
}