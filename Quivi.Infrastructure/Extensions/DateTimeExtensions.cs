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
    }
}