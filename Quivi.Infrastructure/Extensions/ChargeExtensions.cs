using Quivi.Domain.Entities.Charges;

namespace Quivi.Infrastructure.Extensions
{
    public static class ChargeExtensions
    {
        public static bool IsTopUp(this Charge c) => c.Deposit?.DepositCapture!.Person!.SubMerchantId.HasValue == false;
    }
}