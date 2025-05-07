namespace Quivi.Infrastructure.Abstractions.Services
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
    }

    public interface ILogger
    {
        void LogException(Exception ex);

        void Log(string message, LogLevel severity);
    }
}
