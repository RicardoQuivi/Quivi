using Quivi.Infrastructure.Pos.Facturalusa.Models.Items;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.External
{
    public class InvoiceItem
    {
        public InvoiceItem(ItemType type, bool isGeneric = false)
        {
            Type = type;
            IsGeneric = isGeneric;
        }

        public string? Reference { get; set; }
        public string? CorrelationId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        private decimal _taxPercentage;
        public decimal TaxPercentage
        {
            get => _taxPercentage;
            set => _taxPercentage = Math.Round(value, 2);
        }

        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Set <c>true</c> when the item represent a generic item (Diversos).
        /// </summary>
        public bool IsGeneric { get; }

        public ItemType Type { get; }
    }
}
