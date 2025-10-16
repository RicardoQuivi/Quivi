using Quivi.Domain.Entities.Charges;

namespace Quivi.Domain.Repositories.EntityFramework.Models
{
    public class PartnerChargeMethodSales : ASales
    {
        public ChargePartner ChargePartner { get; init; }
        public ChargeMethod ChargeMethod { get; init; }

        public int TotalSuccess { get; init; }
        public int TotalFailed { get; init; }
        public int TotalProcessing { get; init; }
    }
}