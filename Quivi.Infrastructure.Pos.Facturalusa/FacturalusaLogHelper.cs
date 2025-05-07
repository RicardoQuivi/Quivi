using Newtonsoft.Json;
using Quivi.Infrastructure.Abstractions.Services;

namespace Quivi.Infrastructure.Pos.Facturalusa
{
    internal static class FacturalusaLogHelper
    {
        public static async Task<string> Log(this ILogger logger, HttpRequestMessage request, HttpResponseMessage response, LogLevel level)
        {
            var requestUri = request.RequestUri;
            var requestBody = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));

            var responseStatus = response.StatusCode;
            var responseBody = await (response.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));

            var json = new
            {
                Request = new
                {
                    Uri = requestUri,
                    Body = requestBody,
                    Method = response.RequestMessage!.Method.Method,
                },
                Response = new
                {
                    Status = responseStatus,
                    Body = responseBody,
                },
            };
            var message = JsonConvert.SerializeObject(json);
            logger.Log(message, level);
            return responseBody;
        }
    }
}