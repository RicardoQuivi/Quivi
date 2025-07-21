using Quivi.Infrastructure.Abstractions.Pos.Invoicing;

namespace Quivi.Application.Extensions.Pos
{
    public static class InvoiceGatewayExtensions
    {
        public static string BuildCompleteSerieCode(this IInvoiceGateway invoiceGateway, string serieCode, string? prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return $"{serieCode}{invoiceGateway.GatewayCode}";
            return $"{serieCode}-{prefix}{invoiceGateway.GatewayCode}";
        }
    }
}