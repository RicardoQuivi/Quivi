using Quivi.Infrastructure.Pos.Facturalusa.Models;

namespace Quivi.Infrastructure.Pos.Facturalusa.Exceptions
{
    public class FacturalusaApiException : Exception
    {
        public string ErrorMessage { get; }
        public ErrorType ErrorType { get; }

        public FacturalusaApiException(ErrorType errorType, string message) : base($"Facturalusa API returned an error! Error: {errorType} - {message}")
        {
            ErrorType = errorType;
            ErrorMessage = message;
        }
    }
}