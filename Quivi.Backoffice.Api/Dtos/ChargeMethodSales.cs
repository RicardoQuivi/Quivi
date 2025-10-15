namespace Quivi.Backoffice.Api.Dtos
{
    public class ChargeMethodSales
    {
        public required string? CustomChargeMethodId { get; init; }

        public required DateTimeOffset From { get; init; }
        public required DateTimeOffset To { get; init; }

        public decimal TotalInvoices { get; init; }
        public decimal TotalBilledAmount { get; init; }
    }
}