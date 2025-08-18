namespace FacturaLusa.v2.Dtos
{
    public record Sale
    {
        public long Id { get; init; }
        public long? SaleReferenceId { get; init; }
        public required DateTime IssueDate { get; init; }
        public DateTime? DueDate { get; init; }
        public required long SerieId { get; init; }
        public long DocumentTypeId { get; init; }
        public required string DocumentFullNumber { get; init; }
        public decimal GrossTotal { get; init; }
        public decimal TotalDiscount { get; init; }
        public decimal NetTotal { get; init; }
        public decimal TotalBaseVat { get; init; }
        public decimal TotalVat { get; init; }
        public decimal TotalShipping { get; init; }
        public decimal GrandTotal { get; init; }
        public decimal GrandTotalAtExchangeRate { get; init; }
        public decimal FinalFinancialDiscount { get; init; }
        public decimal FinalGlobalDiscount { get; init; }
        public decimal FinalGlobalDiscountValue { get; init; }

        public long CustomerId { get; init; }
        public string? CustomerCode { get; init; }
        public string? CustomerName { get; init; }
        public string? CustomerVatNumber { get; init; }
        public string? CustomerCountry { get; init; }
        public string? CustomerCity { get; init; }
        public string? CustomerAddress { get; init; }
        public string? CustomerPostalCode { get; init; }
        public string? CustomerDeliveryAddressCountry { get; init; }
        public string? CustomerDeliveryAddressCity { get; init; }
        public string? CustomerDeliveryAddressAddress { get; init; }
        public string? CustomerDeliveryAddressPostalCode { get; init; }

        public string? CompanyName { get; init; }
        public string? CompanyVatNumber { get; init; }
        public string? CompanyCountry { get; init; }
        public string? CompanyCity { get; init; }
        public string? CompanyAddress { get; init; }
        public string? CompanyPostalCode { get; init; }

        public long? PaymentMethodId { get; init; }
        public long? PaymentConditionId { get; init; }
        public long? ShippingModeId { get; init; }
        public decimal? ShippingValue { get; init; }
        public long? ShippingVatId { get; init; }
        public long? PriceId { get; init; }
        public required long CurrencyId { get; init; }
        public decimal? CurrencyExchangeRate { get; init; }
        public required VatType VatType { get; init; }
        public string? Observations { get; init; }

        public decimal IrsRetentionApply { get; init; }
        public decimal IrsRetentionBase { get; init; }
        public decimal IrsRetentionTotal { get; init; }
        public decimal IrsRetentionTax { get; init; }

        public string? UrlFile { get; init; }
        public required DocumentFormat FileFormat { get; init; }

        public bool EmailSent { get; init; }
        public bool SmsSent { get; init; }
        public SaleStatus Status { get; init; }

        public long? VehicleId { get; init; }
        public long? EmployeeId { get; init; }
        public DateTime? WaybillShippingDate { get; init; }
        public DateTime? CanceledAt { get; init; }
        public string? CanceledReason { get; init; }
        public bool? WaybillGlobal { get; init; }

        public required string ATCode { get; init; }
        public required string ATMessage { get; init; }

        public required IEnumerable<SaleItem> Items { get; init; }
        public required SaleSerie Serie { get; init; }
        public SalePaymentMethod? PaymentMethod { get; init; }
        public required SaleCustomer Customer { get; init; }

        public SaleReference? SaleReference { get; init; }
    }

    public class SaleSerie
    {
        public required long Id { get; init; }
        public required string Description { get; init; }
        public long ValidUntilYear { get; init; }
    }

    public class SalePaymentMethod
    {
        public required long Id { get; init; }
        public required string Description { get; init; }
        public string? EnglishDescription { get; init; }
    }

    public class SaleCustomer
    {
        public long Id { get; init; }
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? VatNumber { get; init; }
        public string? Country { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? PostalCode { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? MobilePhone { get; init; }
        public long? CurrencyId { get; init; }
        public long? PaymentMethodId { get; init; }
        public long? PaymentConditionId { get; init; }
        public long? ShippingModeId { get; init; }
        public long? PriceId { get; init; }
        public long? EmployeeId { get; init; }
        public required CustomerType Type { get; init; }
        public required VatType VatType { get; init; }
        public long? VatExemptionId { get; init; }
        public decimal IrsRetentionTax { get; init; }
        public string? Observations { get; init; }
        public string? OtherContacts { get; init; }
        public IEnumerable<string>? OtherEmails { get; init; }
        public bool ReceiveSms { get; init; }
        public bool ReceiveEmails { get; init; }
        public string? Language { get; init; }
    }

    public class SaleReference
    {
        public long Id { get; init; }
        public required DateTime IssueDate { get; init; }
        public DateTime? DueDate { get; init; }
        public required long SerieId { get; init; }
        public long DocumentTypeId { get; init; }
        public required string DocumentFullNumber { get; init; }
        public decimal GrandTotal { get; init; }
        public required SaleSerie Serie { get; init; }
    }
}