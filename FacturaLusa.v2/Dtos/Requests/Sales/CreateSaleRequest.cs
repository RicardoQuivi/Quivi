namespace FacturaLusa.v2.Dtos.Requests.Sales
{
    public class CreateSaleRequest
    {
        public long? SaleReferenceId { get; init; }
        public required DateTime IssueDate { get; init; }
        public DateTime? DueDate { get; init; }
        public required DocumentType DocumentType { get; init; }
        public long? SerieId { get; init; }
        public decimal? FinancialDiscountAmount { get; init; }
        public decimal? FinancialGlobalDiscountPercentage { get; init; }
        public required long CustomerId { get; init; }
        public string? VatNumber { get; init; }

        public string? Address { get; init; }
        public string? City { get; init; }
        public string? PostalCode { get; init; }
        public string? Country { get; init; }

        public string? DeliveryAddressAddress { get; init; }
        public string? DeliveryAddressCity { get; init; }
        public string? DeliveryAddressPostalCode { get; init; }
        public string? DeliveryAddressCountry { get; init; }

        public long? PaymentMethodId { get; init; }
        public long? PaymentConditionId { get; init; }
        public long? ShippingModeId { get; init; }
        public decimal? ShippingAmount { get; init; }
        public long? ShippingVatId { get; init; }
        public long? PriceId { get; init; }
        public long? CurrencyId { get; init; }
        public decimal? CurrencyExchangeRate { get; init; }
        public required VatType VatType { get; init; }
        public string? Observations { get; init; }
        public decimal? IrsRetentionTax { get; init; }
        public long? VehicleId { get; init; }
        public long? EmployeeId { get; init; }
        public DateTime? WaybillShippingDate { get; init; }
        public bool? WaybillGlobal { get; init; }
        public long? LocationOriginId { get; init; }
        public long? LocationDestinyId { get; init; }
        public string? CargoLocation { get; init; }
        public string? DischargeLocation { get; init; }
        public DateTime? CargoDate { get; init; }
        public DateTime? DischargeDate { get; init; }
        public required IEnumerable<CreateSaleItem> Items { get; init; }
        public string? Language { get; init; }
        public DocumentFormat? Format { get; init; }
        public int? PaperSize { get; init; }
        public int? PaperLeftMargin { get; init; }
        public int? PaperRightMargin { get; init; }
        public int? PaperTopMargin { get; init; }
        public int? PaperBottomMargin { get; init; }
        public bool? ForcePrint { get; init; }
        public bool? ForceSendEmail { get; init; }
        public bool? ForceSendSms { get; init; }
        public bool? ForceSign { get; init; }
        public string? CallbackUrl { get; init; }
        public string? Reference { get; init; }
        public required SaleStatus Status { get; init; }
    }

    public class CreateSaleItem
    {
        public required long Id { get; init; }
        public string? Details { get; init; }
        public required decimal Price { get; init; }
        public required decimal Quantity { get; init; }
        public required decimal Discount { get; init; }
        public required long VatRateId { get; init; }
        public VatExemptionType? VatExemption { get; init; }
    }
}
