using Quivi.Infrastructure.Abstractions.Services;

namespace Quivi.Infrastructure.Services
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message, LogLevel severity)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{severity}] {message}");
        }

        public void LogException(Exception ex)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [EXCEPTION] {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}