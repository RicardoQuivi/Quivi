namespace Quivi.Infrastructure.Pos.Facturalusa.Models.External
{
    public abstract class ADocument
    {
        public DateTime CreatedDateLocal { get; set; }
        public string? DocumentId { get; set; }
        public required string SerieCode { get; set; }
        public required string PaymentMethodCode { get; set; }
        public string? Notes { get; set; }
        public PriceType PricesType { get; set; }
        public required IEnumerable<InvoiceItem> Items { get; set; }
    }

    public enum PriceType
    {
        /// <summary>
        /// Prices already includes taxes.
        /// </summary>
        IncludedTaxes = 0,

        /// <summary>
        /// Prices are raw without the taxes value.
        /// </summary>
        NotIncludedTaxes = 1,
    }
}