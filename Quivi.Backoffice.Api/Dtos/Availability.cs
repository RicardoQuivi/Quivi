namespace Quivi.Backoffice.Api.Dtos
{
    public class Availability
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required bool AutoAddNewMenuItems { get; set; }
        public required bool AutoAddNewChannelProfiles { get; set; }
        public required IEnumerable<WeeklyAvailability> WeeklyAvailabilities { get; init; }
    }

    /// <summary>
    /// Adds a weekly availability on the provided range. If <c>StartAt</c> is <c>TimeSpan.Zero</c>, then 
    /// it means it is available from 00:00:00 of Sunday. The maximum value <c>StartAt</c> and <c>EndAt</c>
    /// is 7 days (128 hours). For instance, if <c>StartAt</c> is equal to 24:00:00 (24 hours) and<c>EndAt</c>
    /// is equal to 104:00:00, then this represents availability from Monday 00:00:00 until Saturday 00:00:00
    /// </summary>
    public class WeeklyAvailability
    {
        /// <summary>
        /// The start of this duration in relation to Sunday 00:00:00
        /// </summary>
        public required TimeSpan StartAt { get; init; }
        /// <summary>
        /// The end of this duration in relation to Sunday 00:00:00
        /// </summary>
        public required TimeSpan EndAt { get; init; }
    }
}