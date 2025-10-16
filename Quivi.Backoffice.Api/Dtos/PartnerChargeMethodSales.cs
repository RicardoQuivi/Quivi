using Quivi.Domain.Entities.Charges;

namespace Quivi.Backoffice.Api.Dtos
{
    public class PartnerChargeMethodSales
    {
        public required ChargePartner ChargePartner { get; init; }
        public required ChargeMethod ChargeMethod { get; init; }

        public required DateTimeOffset From { get; init; }
        public required DateTimeOffset To { get; init; }

        public int Total { get; init; }
        public int TotalSuccess { get; init; }
        public int TotalFailed { get; init; }
        public int TotalProcessing { get; init; }
    }
}