namespace Quivi.Infrastructure.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToCron(this TimeSpan periodRecurrence)
        {
            if (periodRecurrence.Hours >= 1)
                return $"{periodRecurrence.Minutes} */{periodRecurrence.Hours} * * *";

            if (periodRecurrence.Minutes >= 1)
                return $"*/{periodRecurrence.Minutes} * * * *";

            return $"*/{periodRecurrence.Seconds} * * * * *";
        }
    }
}