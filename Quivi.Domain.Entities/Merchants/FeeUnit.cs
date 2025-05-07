namespace Quivi.Domain.Entities.Merchants
{
    public enum FeeUnit
    {
        /// <summary>
        /// Represents a percentage. Tipically between 0 and 100.
        /// </summary>
        Percentage = 0,
        /// <summary>
        /// Represents a percentage. Tipically between 0 and 1.
        /// </summary>
        PercentageFraction = 1,
        /// <summary>
        /// Represents an absolute value.
        /// </summary>
        Absolute = 2,
    }
}
