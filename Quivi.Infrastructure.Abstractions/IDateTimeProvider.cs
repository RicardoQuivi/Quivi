namespace Quivi.Infrastructure.Abstractions
{
    public interface IDateTimeProvider
    {
        DateTime GetUtcNow();
    }
}
