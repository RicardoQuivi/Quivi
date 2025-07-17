using Quivi.Infrastructure.Abstractions;

namespace Quivi.Infrastructure
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetUtcNow() => DateTime.UtcNow;
        public DateTime GetNow(string? timezoneId)
        {
            if (string.IsNullOrEmpty(timezoneId))
                timezoneId = "GMT Standard Time";

            var tzi = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
        }
    }
}