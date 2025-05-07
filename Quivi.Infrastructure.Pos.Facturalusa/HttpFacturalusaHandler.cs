using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quivi.Infrastructure.Abstractions.Services;
using Quivi.Infrastructure.Pos.Facturalusa.Exceptions;
using Quivi.Infrastructure.Pos.Facturalusa.Models;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Quivi.Infrastructure.Pos.Facturalusa
{
    internal class HttpFacturalusaHandler : HttpClientHandler
    {
        private readonly ILogger logger;
        private readonly string accessToken;

        private class ErrorResponse : AResponseBase 
        {
            /// <summary>
            /// <c>True</c> is success. <c>False</c> if failed.
            /// </summary>
            [JsonProperty("status")]
            public bool Success { get; set; }

            /// <summary>
            /// The error message if request failed.
            /// </summary>
            [JsonProperty("message")]
            public string ErrorMessage
            {
                get => _message;
                private set
                {
                    _message = value;
                    ErrorMessageType = JsonConvert.DeserializeObject<ErrorType>($"\"{value}\"");
                }
            }

            private string _message = string.Empty;

            [JsonIgnore]
            public ErrorType ErrorMessageType { get; private set; }
        }

        public HttpFacturalusaHandler(ILogger logger, string accessToken)
        {
            this.logger = logger;
            this.accessToken = accessToken;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpStatusCode[] allowedStatusCode = new[] { HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden };

            await HandleRequest(request);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!allowedStatusCode.Contains(response.StatusCode))
            {
                var message = await logger.Log(request, response, LogLevel.Error);
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(message)!;
                    string pattern = @"Já efectuou muitas tentativas, aguarde (\d+) segundos";
                    Match match = Regex.Match(errorResponse.ErrorMessage, pattern);
                    if (match.Success)
                    {
                        int seconds = int.Parse(match.Groups[1].Value) + 2;
                        await Task.Delay(TimeSpan.FromSeconds(seconds)).ConfigureAwait(false);
                        return await SendAsync(request, cancellationToken).ConfigureAwait(false);
                    }
                    throw new Exception($"Unhandled Exception: {message}");
                }
                catch
                {
                    throw new Exception($"Unhandled Exception: {message}");
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                await logger.Log(request, response, response.StatusCode == HttpStatusCode.Unauthorized ? LogLevel.Error : LogLevel.Warn);
                
                var jsonResponse = await (response.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(jsonResponse)!;
                throw new FacturalusaApiException(response.StatusCode == HttpStatusCode.Unauthorized ? ErrorType.Unauthorized : errorResponse.ErrorMessageType, errorResponse.ErrorMessage);
            }

            await logger.Log(request, response, LogLevel.Info);
            return response;
        }

        private async Task HandleRequest(HttpRequestMessage request) 
        {
            // Read the original payload
            var json = await request.Content!.ReadAsStringAsync();

            // Convert to JObject
            var data = JsonConvert.DeserializeObject<JObject>(json)!;

            // Add the api_token
            data["api_token"] = accessToken;

            // Replace the request content
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            request.Headers.Add("Accept", "application/json");
        }
    }
}