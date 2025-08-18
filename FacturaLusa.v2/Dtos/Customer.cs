namespace FacturaLusa.v2.Dtos
{
    public class Customer
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
}