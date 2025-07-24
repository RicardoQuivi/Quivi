using Quivi.Infrastructure.Abstractions;

namespace Quivi.Infrastructure
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetUtcNow() => DateTime.UtcNow;
    }
}