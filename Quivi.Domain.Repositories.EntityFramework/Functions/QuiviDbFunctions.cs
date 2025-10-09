namespace Quivi.Domain.Repositories.EntityFramework.Functions
{
    public static class QuiviDbFunctions
    {
        public static DateTimeOffset ToTimeZone(string date, string? timezone) => throw new NotSupportedException();
        public static DateTimeOffset ToTimeZone(DateTime date, string? timezone) => throw new NotSupportedException();

        public static int ToWeeklyAvailabilityInSeconds(DateTimeOffset date) => (int)date.ToWeeklyAvailability().TotalSeconds;

        private static TimeSpan ToWeeklyAvailability(this DateTimeOffset time)
        {
            var aux = time.UtcDateTime.AddDays(-1 * (int)time.DayOfWeek).Date;
            return time - aux;
        }
    }
}