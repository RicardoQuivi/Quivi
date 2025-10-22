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

        public static DateTime GetSettlementFromDateUtc(this DateTime date, string? timezoneId)
        {
            if (date.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Date should be UTC", nameof(date));

            var merchantLocalDate = date.ToTimeZone(timezoneId);

            var offset = QuiviConstants.SettlementOffset;
            var startdate = merchantLocalDate.Date.Add(offset);
            if (merchantLocalDate.Add(-offset).Date != startdate.Date)
                startdate = startdate.AddDays(-1).Date.Add(offset);

            return startdate.ToUniversalTime();
        }

        public static DateTime FromUtcToTimeZone(this DateTime utcDatetime, string timeZoneId = "GMT Standard Time")
        {
            if (utcDatetime.Kind != DateTimeKind.Utc)
                throw new ArgumentException($"The input datetime is not UTC DateKind. The current kind is '{utcDatetime.Kind}'");

            if (string.IsNullOrEmpty(timeZoneId))
                timeZoneId = "GMT Standard Time";
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDatetime, tzi);
        }
    }
}