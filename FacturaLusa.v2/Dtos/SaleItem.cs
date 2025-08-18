namespace FacturaLusa.v2.Dtos
{
    public class SaleItem
    {
        public long Id { get; init; }
        public long ItemId { get; init; }
        public string? ItemDetails { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal Quantity { get; init; }
        public decimal Discount { get; init; }
        public decimal GrossTotal { get; init; }
        public decimal NetTotal { get; init; }
        public decimal TotalBaseVat { get; init; }
        public decimal TotalVat { get; init; }
        public decimal TotalDiscount { get; init; }
        public decimal GrandTotal { get; init; }
        public long VatRateId { get; init; }
        public long UnitId { get; init; }
        public long VatExemptionId { get; init; }
        public required SaleItemItem Item { get; init; }
        public required SaleItemUnit Unit { get; init; }
        public required SaleItemVatRate VatRate { get; init; }
        public required SaleItemVatExemption VatExemption { get; init; }
    }

    public class SaleItemItem
    {
        public long Id { get; init; }
        public required string Reference { get; init; }
        public required string Description { get; init; }
        public string? Details { get; init; }
        public ItemType Type { get; init; }
    }

    public class SaleItemUnit
    {
        public long Id { get; init; }
        public required string Description { get; init; }
        public required string Symbol { get; init; }
    }

    public class SaleItemVatRate
    {
        public long Id { get; init; }
        public required string Description { get; init; }
        public required TaxType Type { get; init; }
        public required decimal TaxPercentage { get; init; }
        public SaftRegion? SaftRegion { get; init; }
    }

    public class SaleItemVatExemption
    {
        public long Id { get; init; }
        public required string Code { get; init; }
        public required string Description { get; init; }

    }
}