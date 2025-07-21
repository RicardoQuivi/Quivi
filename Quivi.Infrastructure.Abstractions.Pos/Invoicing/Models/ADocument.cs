namespace Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models
{
    public abstract class ADocument
    {
        public DateTime CreatedDateUtc { get; set; }
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

    public class InvoiceItem : BaseItem
    {
        public InvoiceItem(InvoiceItemType type) : base(type)
        {
        }

        public decimal Quantity { get; set; }
        public decimal DiscountPercentage { get; set; }
    }

    public class ProductItem : BaseItem
    {
        public ProductItem(InvoiceItemType type) : base(type)
        {
        }

        public bool IsDeleted { get; set; }
    }

    public abstract class BaseItem
    {
        public BaseItem(InvoiceItemType type)
        {
            Type = type;
        }

        public string? Reference { get; set; }
        public required string CorrelationId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal Price { get; set; }
        public InvoiceItemType Type { get; }
    }

    public enum InvoiceItemType
    {
        ProcessedProducts = 0,
        Services = 1,
        Generic = 2,
    }

    public class Customer
    {
        public Customer(CustomerType type)
        {
            Type = type;
        }

        public CustomerType Type { get; }
        public string? Code { get; set; }
        public string? VatNumber { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? MobileNumber { get; set; }
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
    }

    public enum CustomerType
    {
        FinalConsumer,
        Personal,
        Company,
    }

    public enum DocumentFileFormat
    {
        A4 = 0,
        POS,
        EscPOS
    }
}
