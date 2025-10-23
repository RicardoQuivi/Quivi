using ComplyPay.Abstractions;
using ComplyPay.Dtos;
using ComplyPay.Dtos.Requests;
using ComplyPay.Dtos.Responses;
using ComplyPay.Exceptions;
using System.Text;
using System.Text.Json;

namespace ComplyPay
{
    public class ComplyPayApi : IComplyPayApi
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        public Lazy<Uri> BaseUri { get; }
        public string Version { get; }

        public ComplyPayApi(string host, string version = "v1")
        {
            BaseUri = new Lazy<Uri>(() => new Uri(host, UriKind.Absolute));
            Version = version;
        }

        private static TObject Deserialize<TObject>(string rawData, TObject anonymousObj) => JsonSerializer.Deserialize<TObject>(rawData, jsonSerializerOptions)!;

        public async Task<AuthenticateJwtResponse> AuthenticateJwt(AuthenticateJwtRequest request)
        {
            using (var client = new HttpClient())
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUri.Value, $"{Version}/user-logins/authenticate-jwt")))
            {
                requestMessage.Content = new StringContent(JsonSerializer.Serialize(request, jsonSerializerOptions), Encoding.UTF8, "application/json");
                var httpResponse = await client.SendAsync(requestMessage);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                httpResponse.EnsureSuccessStatusCode();

                var rawResponse = await httpResponse.Content.ReadAsStringAsync();
                if (rawResponse == null)
                    throw new Exception("Endpoint returned no response");

                var response = Deserialize(rawResponse, new
                {
                    status_message = "",
                    token = "",
                    refresh_token = "",
                });
                if (response == null)
                    throw new Exception("Endpoint returned no response");

                return new AuthenticateJwtResponse
                {
                    RefreshToken = response.refresh_token,
                    StatusMessage = response.status_message,
                    Token = response.token,
                };
            }
        }

        public async Task<JwtRefreshResponse> JwtRefresh(JwtRefreshRequest request)
        {
            using (var client = new HttpClient())
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUri.Value, $"{Version}/user-logins/jwt-refresh")))
            {
                requestMessage.Headers.Add("JwtAuthorization", request.RefreshToken);

                var httpResponse = await client.SendAsync(requestMessage);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();
                httpResponse.EnsureSuccessStatusCode();

                var rawResponse = await httpResponse.Content.ReadAsStringAsync();
                if (rawResponse == null)
                    throw new Exception("Endpoint returned no response");

                var response = Deserialize(rawResponse, new
                {
                    status_message = "",
                    token = "",
                    refresh_token = "",
                });
                if (response == null)
                    throw new Exception("Endpoint returned no response");

                return new JwtRefreshResponse
                {
                    RefreshToken = response.refresh_token,
                    StatusMessage = response.status_message,
                    Token = response.token,
                };
            }
        }

        public async Task<GetVendorAccountsResponse> GetVendorAccounts(string token, GetVendorAccountsRequest request)
        {
            using (var client = new HttpClient())
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUri.Value, $"{Version}/vendor-accounts/search?page={request.Page}&limit={request.PageSize}")))
            {
                requestMessage.Headers.Add("JwtAuthorization", token);

                var payload = JsonSerializer.Serialize(new
                {
                    status = request.Status,
                    brn = request.BusinessRegistrations,
                    vat = request.VATs,
                    reference_id = new
                    {
                        In = request.ReferenceIds,
                    },
                }, jsonSerializerOptions);
                requestMessage.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                var httpResponse = await client.SendAsync(requestMessage);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                httpResponse.EnsureSuccessStatusCode();

                var rawResponse = await httpResponse.Content.ReadAsStringAsync();
                if (rawResponse == null)
                    throw new Exception("Endpoint returned no response");

                var response = Deserialize(rawResponse, new
                {
                    status_message = "",
                    size = 0,
                    vendor_accounts = new[]
                    {
                        new
                        {
                            account_identifier = "",
                            type = "",
                            company = 0,
                            description = "",
                            payout_iban = "",
                            viban = "",
                            currencies = Enumerable.Empty<string>(),
                            vendor_information = new
                            {
                                reference_id = "",
                            },
                        },
                    }.AsEnumerable(),
                });
                if (response == null)
                    throw new Exception("Endpoint returned no response");

                return new GetVendorAccountsResponse
                {
                    StatusMessage = response.status_message,
                    Size = response.size,
                    VendorAccounts = response.vendor_accounts.Select(x => new Dtos.VendorAccount
                    {
                        AccountIdentifier = x.account_identifier,
                        Company = x.company,
                        Currencies = x.currencies,
                        Description = x.description,
                        PayoutIban = x.payout_iban,
                        Type = x.type,
                        VirtualIban = x.viban,
                        ReferenceId = x.vendor_information.reference_id,
                    }).ToList(),
                };
            }
        }

        public async Task<CreatePaymentResponse> CreatePayment(string token, CreatePaymentRequest request)
        {
            using (var client = new HttpClient())
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUri.Value, $"{Version}/payment-objects")))
            {
                requestMessage.Headers.Add("JwtAuthorization", token);
                if (string.IsNullOrWhiteSpace(request.IdempotencyKey) == false)
                    requestMessage.Headers.Add("Idempotency-Key", request.IdempotencyKey);

                var serializedMessage = JsonSerializer.Serialize(new
                {
                    amount = request.Amount,
                    currency = request.Currency,
                    payer = new
                    {
                        id = request.Payer.Id,
                        type = ToString(request.Payer.Type),
                    },
                    payee = new
                    {
                        id = request.Payee.Id,
                    },
                    iban_memo = request.Memo,
                    description = request.Description,
                    payment_flow_configuration = ToString(request.PaymentFlowType),
                    metadata = request.Metadata,
                }, jsonSerializerOptions);
                requestMessage.Content = new StringContent(serializedMessage, Encoding.UTF8, "application/json");
                var httpResponse = await client.SendAsync(requestMessage);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                    throw new PaymentAlreadyExistsException();

                httpResponse.EnsureSuccessStatusCode();

                var rawResponse = await httpResponse.Content.ReadAsStringAsync();
                if (rawResponse == null)
                    throw new Exception("Endpoint returned no response");

                var response = Deserialize(rawResponse, new
                {
                    status_message = "",
                    id = 0,
                });
                if (response == null)
                    throw new Exception("Endpoint returned no response");

                return new CreatePaymentResponse
                {
                    StatusMessage = response.status_message,
                    Id = response.id,
                };
            }
        }

        public async Task<PayoutAllResponse> PayoutAll(string token)
        {
            using (var client = new HttpClient())
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUri.Value, $"{Version}/vendor-accounts/payout-all")))
            {
                requestMessage.Headers.Add("JwtAuthorization", token);

                var httpResponse = await client.SendAsync(requestMessage);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                httpResponse.EnsureSuccessStatusCode();

                var rawResponse = await httpResponse.Content.ReadAsStringAsync();
                if (rawResponse == null)
                    throw new Exception("Endpoint returned no response");

                var response = Deserialize(rawResponse, new
                {
                    status_message = "",
                    payment_object_ids = Enumerable.Empty<int>(),
                });
                if (response == null)
                    throw new Exception("Endpoint returned no response");

                return new PayoutAllResponse
                {
                    StatusMessage = response.status_message,
                    PayoutIds = response.payment_object_ids,
                };
            }
        }

        public async Task<GetWalletBallanceResponse> GetWalletBallance(string token, GetWalletBallanceRequest request)
        {
            using (var client = new HttpClient())
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(BaseUri.Value, $"{Version}/payment-accounts/{ToString(request.AccountType)}/get-balances")))
            {
                requestMessage.Headers.Add("JwtAuthorization", token);

                var httpResponse = await client.SendAsync(requestMessage);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                httpResponse.EnsureSuccessStatusCode();

                var rawResponse = await httpResponse.Content.ReadAsStringAsync();
                if (rawResponse == null)
                    throw new Exception("Endpoint returned no response");

                var response = Deserialize(rawResponse, new
                {
                    status_message = "",
                    balance = new[]{
                        new
                        {
                            currency = "",
                            balance = 1,
                        },
                    }
                });
                if (response == null)
                    throw new Exception("Endpoint returned no response");

                return new GetWalletBallanceResponse
                {
                    StatusMessage = response.status_message,
                    Balance = response.balance.Select(b => new Balance
                    {
                        Amount = b.balance,
                        Currency = b.currency,
                    }).First(),
                };
            }
        }

        private string ToString(PaymentFlowType paymentFlowType)
        {
            switch (paymentFlowType)
            {
                case PaymentFlowType.Platform: return "PLATFORM";
                case PaymentFlowType.PlatformNo4Eyes: return "PLATFORM_NO_FOUR_EYES_APPROVAL";
            }
            throw new NotImplementedException();
        }

        private string ToString(AccountType type)
        {
            switch (type)
            {
                case AccountType.Treasury: return "TREASURY";
                case AccountType.Split: return "SPLIT";
            }
            throw new NotImplementedException();
        }
    }
}
