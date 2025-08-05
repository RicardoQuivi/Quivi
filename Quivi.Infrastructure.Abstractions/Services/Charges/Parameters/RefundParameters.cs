namespace Quivi.Infrastructure.Abstractions.Services.Charges.Parameters
{
    public class RefundParameters
    {
        public int ChargeId { get; init; }
        public int? EmployeeId { get; init; }
        public int? MerchantId { get; init; }
        public decimal? Amount { get; init; }
        public bool IsCancellation { get; init; }
        public string? Reason { get; init; }
    }
}