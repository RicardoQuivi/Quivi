using Quivi.Domain.Entities.Merchants;
using System.Globalization;

namespace Quivi.Application.Extensions
{
    public static class FeesExtensions
    {
        public static decimal GetFee(this decimal val, decimal? fee, FeeUnit unit) => GetFee(val, fee ?? 0.0M, unit);

        public static decimal GetFee(this decimal val, decimal fee, FeeUnit unit)
        {
            switch (unit)
            {
                case FeeUnit.Percentage: return val * fee / 100;
                case FeeUnit.PercentageFraction: return val * fee;
                case FeeUnit.Absolute: return fee;
            }
            throw new NotImplementedException();
        }

        public static decimal ApplyFee(this decimal val, decimal? fee, FeeUnit unit) => ApplyFee(val, fee ?? 0.0M, unit);

        public static decimal ApplyFee(this decimal val, decimal fee, FeeUnit unit)
        {
            switch (unit)
            {
                case FeeUnit.Percentage: return val * (1 + fee / 100);
                case FeeUnit.PercentageFraction: return val * (1 + fee);
                case FeeUnit.Absolute: return val + fee;
            }
            throw new NotImplementedException();
        }

        public static string ToString(this decimal? fee, FeeUnit unit, IFormatProvider formatProvider) => ToString(fee ?? 0.0M, unit, formatProvider);

        public static string ToString(this decimal fee, FeeUnit unit, IFormatProvider formatProvider)
        {
            switch (unit)
            {
                case FeeUnit.Percentage: return (fee / 100).ToString("P2", formatProvider);
                case FeeUnit.PercentageFraction: return fee.ToString("P2", formatProvider);
                case FeeUnit.Absolute: return fee.ToString("C2", formatProvider);
            }
            throw new NotImplementedException();
        }

        public static string ToString(this decimal? fee, FeeUnit unit) => ToString(fee ?? 0.0M, unit);

        public static string ToString(this decimal fee, FeeUnit unit, CultureInfo? cultureInfo = null) => ToString(fee, unit, cultureInfo ?? CultureInfo.CreateSpecificCulture("pt-PT"));
    }
}