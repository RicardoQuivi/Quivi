namespace Quivi.Infrastructure.Extensions
{
    public static class PriceHelper
    {
        public static decimal GetMinimumValue(int decimalPlaces)
        {
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places must be non-negative.");
            return 1m / (decimal)Math.Pow(10, decimalPlaces);
        }

        public static decimal CalculateDiscountPercentage(decimal originalPrice, decimal priceAfterDiscount)
        {
            return originalPrice == 0 ? 0 : 100 - priceAfterDiscount / originalPrice * 100;
        }

        public static decimal CalculatePriceAfterDiscount(decimal originalPrice, decimal discountPercentage)
        {
            return originalPrice - (originalPrice * (discountPercentage / 100));
        }

        public static decimal CalculateOriginalPrice(decimal priceAfterDiscount, decimal discountPercentage)
        {
            return discountPercentage == 100 ? 0 : 100 * priceAfterDiscount / (100 - discountPercentage);
        }
    }
}
