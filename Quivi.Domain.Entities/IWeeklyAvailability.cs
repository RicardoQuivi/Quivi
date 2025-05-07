namespace Quivi.Domain.Entities
{
    /// <summary>
    /// Represents a weekly availability. If <c>>StartAt</c> is <c>TimeSpan.Zero</c>, then 
    /// it means it is available from 00:00:00 of Sunday. The maximum value <c>EndAt</c>
    /// is 7 days (128 hours). For instance, if <c>StartAt</c> is equal to 24:00:00 (24 hours) and<c>EndAt</c>
    /// is equal to 104:00:00, then this represents availability from Monday 00:00:00 until Saturday 00:00:00
    /// </summary>
    public interface IWeeklyAvailability
    {
        TimeSpan StartAt { get; set; }
        TimeSpan EndAt { get; set; }
    }

    public interface IDomainWeeklyAvailability
    {
        int StartAtSeconds { get; set; }
        int EndAtSeconds { get; set; }
    }
}
