using Quivi.Infrastructure.Abstractions.Services;

namespace Quivi.Infrastructure.Services
{
    public class NoLogger : ILogger
    {
        public void Log(string message, LogLevel severity)
        {
        }

        public void LogException(Exception ex)
        {
        }
    }
}
