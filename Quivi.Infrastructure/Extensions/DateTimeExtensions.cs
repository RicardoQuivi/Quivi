namespace Quivi.Infrastructure.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToTimeZone(this DateTime date, string? timezoneId)
        {
            if (string.IsNullOrEmpty(timezoneId))
                timezoneId = "GMT Standard Time";

            var tzi = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(date, tzi);
        }
    }
}