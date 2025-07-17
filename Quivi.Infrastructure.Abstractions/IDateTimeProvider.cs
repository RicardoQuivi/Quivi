namespace Quivi.Infrastructure.Abstractions
{
    public interface IDateTimeProvider
    {
        DateTime GetUtcNow();
        DateTime GetNow(string? timezoneId);
    }
}
