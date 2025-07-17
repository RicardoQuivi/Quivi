using Quivi.Domain.Entities.Pos;

namespace Quivi.Application.Extensions
{
    public static class PosChargeExtensions
    {
        public enum PosChargeType
        {
            FreePayment,
            PayAtTheTable,
            OrderAndPay,
        }

        public static PosChargeType GetPosChargeType(this PosCharge details)
        {
            if (details.SessionId.HasValue)
                return PosChargeType.PayAtTheTable;

            if (details.PosChargeSelectedMenuItems!.Any())
                return PosChargeType.OrderAndPay;

            return PosChargeType.FreePayment;
        }
    }
}
