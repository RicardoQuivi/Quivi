using FacturaLusa.v2.Converters;
using FacturaLusa.v2.Dtos;
using FacturaLusa.v2.Dtos.Requests.Currencies;
using FacturaLusa.v2.Dtos.Requests.Customers;
using FacturaLusa.v2.Dtos.Requests.Items;
using FacturaLusa.v2.Dtos.Requests.PaymentConditions;
using FacturaLusa.v2.Dtos.Requests.PaymentMethods;
using FacturaLusa.v2.Dtos.Requests.Sales;
using FacturaLusa.v2.Dtos.Requests.Series;
using FacturaLusa.v2.Dtos.Requests.Units;
using FacturaLusa.v2.Dtos.Requests.VatRates;
using FacturaLusa.v2.Dtos.Responses.Currencies;
using FacturaLusa.v2.Dtos.Responses.Customers;
using FacturaLusa.v2.Dtos.Responses.Items;
using FacturaLusa.v2.Dtos.Responses.PaymentConditions;
using FacturaLusa.v2.Dtos.Responses.PaymentMethods;
using FacturaLusa.v2.Dtos.Responses.Sales;
using FacturaLusa.v2.Dtos.Responses.Series;
using FacturaLusa.v2.Dtos.Responses.Units;
using FacturaLusa.v2.Dtos.Responses.VatRates;
using FacturaLusa.v2.Exceptions;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturaLusa.v2
{
    public class FacturaLusaApi : IFacturaLusaApi
    {
        private static readonly TimeZoneInfo LisbonTZ = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "GMT Standard Time" : "Europe/Lisbon");

        private readonly Uri apiAdress;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public FacturaLusaApi(string apiAddress)
        {
            this.apiAdress = new Uri(apiAddress);

            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            };
            jsonSerializerOptions.Converters.Add(new DocumentTypeConverter());
            jsonSerializerOptions.Converters.Add(new VatTypeConverter());
            jsonSerializerOptions.Converters.Add(new SaleStatusConverter());
            jsonSerializerOptions.Converters.Add(new TaxTypeConverter());
            jsonSerializerOptions.Converters.Add(new SaftRegionConverter());
            jsonSerializerOptions.Converters.Add(new ItemTypeConverter());
            jsonSerializerOptions.Converters.Add(new SearchFieldConverter());
            jsonSerializerOptions.Converters.Add(new ClientTypeConverter());
            jsonSerializerOptions.Converters.Add(new VatRateExemptionTypeConverter());
            jsonSerializerOptions.Converters.Add(new DocumentIssueConverter());
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        private async Task<TResponse> Submit<TResponse>(string endpoint, string apiKey, object request, Func<HttpClient, StringContent, Task<HttpResponseMessage>> action)
        {
            var json = JsonSerializer.Serialize(request, jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            httpClient.BaseAddress = apiAdress;
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var response = await action(httpClient, content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new NotAuthorizedException();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new FacturaLusaException("Entity Not Found", ErrorType.EntityXNotExists);

            var rawResponse = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var message = JsonSerializer.Deserialize<string>(rawResponse, jsonSerializerOptions)!;
                var errorType = ErrorTypeMapper.FromMessage(message);
                throw new FacturaLusaException(message, errorType);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new HttpRequestException(rawResponse, null, response.StatusCode);

            var result = JsonSerializer.Deserialize<TResponse>(rawResponse, jsonSerializerOptions) ?? throw new InvalidOperationException("Failed to deserialize response from FacturaLusa API.");
            return result;
        }

        private Task<TResponse> Put<TResponse>(string endpoint, string apiKey, object request) => Submit<TResponse>(endpoint, apiKey, request, (httpClient, content) => httpClient.PutAsync(endpoint, content));
        private Task<TResponse> Post<TResponse>(string endpoint, string apiKey, object request) => Submit<TResponse>(endpoint, apiKey, request, (httpClient, content) => httpClient.PostAsync(endpoint, content));
        private Task<TResponse> Post<TResponse>(string endpoint, string apiKey, object request, TResponse anonymous) => Post<TResponse>(endpoint, apiKey, request);

        #region Units
        public async Task<SearchUnitResponse> SearchUnit(string apiKey, SearchUnitRequest request)
        {
            var result = await Post("api/v2/administration/units/find", apiKey, new
            {
                Value = request.Value,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Symbol = string.Empty,
                Created_at = DateTime.MinValue,
                Updated_at = DateTime.MinValue,
            });

            return new SearchUnitResponse
            {
                Id = result.Id,
                Symbol = result.Symbol,
                Description = result.Description,
                CreatedAt = result.Created_at,
                UpdatedAt = result.Updated_at,
            };
        }

        public async Task<CreateUnitResponse> CreateUnit(string apiKey, CreateUnitRequest request)
        {
            var result = await Post("api/v2/administration/units", apiKey, request, new
            {
                Id = 0L,
                Description = string.Empty,
                Symbol = string.Empty,
                Created_at = DateTime.MinValue,
                Updated_at = DateTime.MinValue,
            });

            return new CreateUnitResponse
            {
                Id = result.Id,
                Symbol = result.Symbol,
                Description = result.Description,
                CreatedAt = result.Created_at,
                UpdatedAt = result.Updated_at,
            };
        }
        #endregion


        #region Vat Rates
        public async Task<SearchVatRateResponse> SearchVatRate(string apiKey, SearchVatRateRequest request)
        {
            var result = await Post("api/v2/administration/vatrates/find", apiKey, new
            {
                Value = request.Value,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Type = TaxType.Isenta,
                Saft_region = SaftRegion.PortugalContinental,
                Tax = string.Empty,
            });

            return new SearchVatRateResponse
            {
                Id = result.Id,
                Description = result.Description,
                Type = result.Type,
                SaftRegion = result.Saft_region,
                TaxPercentage = decimal.Parse(result.Tax),
            };
        }

        public async Task<CreateVatRateResponse> CreateVatRate(string apiKey, CreateVatRateRequest request)
        {
            var result = await Post("api/v2/administration/vatrates", apiKey, new
            {
                Description = request.Description,
                Tax = request.TaxPercentage,
                Type = request.Type,
                Saft_region = request.SaftRegion,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Type = TaxType.Isenta,
                Saft_region = (SaftRegion)SaftRegion.PortugalContinental,
                Tax = string.Empty,
            });

            return new CreateVatRateResponse
            {
                Id = result.Id,
                Description = result.Description,
                Type = result.Type,
                SaftRegion = result.Saft_region,
                TaxPercentage = decimal.Parse(result.Tax),
            };
        }
        #endregion


        #region Items
        public async Task<SearchItemResponse> SearchItem(string apiKey, SearchItemRequest request)
        {
            var result = await Post("api/v2/items/find", apiKey, new
            {
                Value = request.Value,
                Search_in = request.Field,
            }, new
            {
                Id = 0L,
                Reference = string.Empty,
                Description = string.Empty,
                Details = (string?)null,
                Unit = new
                {
                    Id = 0L,
                    Description = string.Empty,
                    Symbol = string.Empty,
                    Created_at = DateTime.MinValue,
                    Updated_at = DateTime.MinValue,
                },
                Vat = new
                {
                    Id = 0L,
                    Description = string.Empty,
                    Type = TaxType.Isenta,
                    Saft_region = SaftRegion.PortugalContinental,
                    Tax = string.Empty,
                },
                Type = ItemType.Service,
                Observations = string.Empty,
            });

            return new SearchItemResponse
            {
                Id = result.Id,
                Reference = result.Reference,
                Description = result.Description,
                Unit = new Unit
                {
                    Id = result.Unit.Id,
                    Symbol = result.Unit.Symbol,
                    Description = result.Unit.Description,
                    CreatedAt = result.Unit.Created_at,
                    UpdatedAt = result.Unit.Updated_at,
                },
                VatRate = new VatRate
                {
                    Id = result.Vat.Id,
                    Description = result.Vat.Description,
                    Type = result.Vat.Type,
                    SaftRegion = result.Vat.Saft_region,
                    TaxPercentage = decimal.Parse(result.Vat.Tax),
                },
                Type = result.Type,
                Details = result.Details,
                Observations = result.Observations,
            };
        }

        public async Task<CreateItemResponse> CreateItem(string apiKey, CreateItemRequest request)
        {
            var result = await Post("api/v2/items", apiKey, new
            {
                Reference = request.Reference,
                Description = request.Description,
                Details = request.Details,
                Category = request.CategoryId,
                Unit = request.UnitId,
                Vat = request.VatRateId,
                Type = request.Type,
                Observations = request.Observations,
            }, new
            {
                Id = 0L,
                Reference = string.Empty,
                Description = string.Empty,
                Details = (string?)null,
                Unit = new
                {
                    Id = 0L,
                    Description = string.Empty,
                    Symbol = string.Empty,
                    Created_at = DateTime.MinValue,
                    Updated_at = DateTime.MinValue,
                },
                Vat = new
                {
                    Id = 0L,
                    Description = string.Empty,
                    Type = TaxType.Isenta,
                    Saft_region = (SaftRegion)SaftRegion.PortugalContinental,
                    Tax = string.Empty,
                },
                Type = ItemType.Service,
                Observations = string.Empty,
            });

            return new CreateItemResponse
            {
                Id = result.Id,
                Reference = result.Reference,
                Description = result.Description,
                Unit = new Unit
                {
                    Id = result.Unit.Id,
                    Symbol = result.Unit.Symbol,
                    Description = result.Unit.Description,
                    CreatedAt = result.Unit.Created_at,
                    UpdatedAt = result.Unit.Updated_at,
                },
                VatRate = new VatRate
                {
                    Id = result.Vat.Id,
                    Description = result.Vat.Description,
                    Type = result.Vat.Type,
                    SaftRegion = result.Vat.Saft_region,
                    TaxPercentage = decimal.Parse(result.Vat.Tax),
                },
                Type = result.Type,
                Details = result.Details,
                Observations = result.Observations,
            };
        }
        #endregion


        #region Currencies
        public async Task<SearchCurrencyResponse> SearchCurrency(string apiKey, SearchCurrencyRequest request)
        {
            var result = await Post("api/v2/administration/currencies/find", apiKey, new
            {
                Value = request.Value,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Symbol = string.Empty,
                Code_iso = string.Empty,
                Exchange_sale = string.Empty,
                Is_default = 1,
                Updated_at = DateTime.MinValue,
                Created_at = DateTime.MinValue,
            });

            return new SearchCurrencyResponse
            {
                Id = result.Id,
                Description = result.Description,
                Symbol = result.Symbol,
                IsoCode = result.Code_iso,
                ExchangeRate = decimal.Parse(result.Exchange_sale),
                IsDefault = result.Is_default == 1,
                CreatedAt = result.Created_at,
                UpdatedAt = result.Updated_at,
            };
        }

        public async Task<CreateCurrencyResponse> CreateCurrency(string apiKey, CreateCurrencyRequest request)
        {
            var result = await Post("api/v2/administration/currencies", apiKey, new
            {
                Description = request.Description,
                Symbol = request.Symbol,
                Code_iso = request.IsoCode,
                Exchange_sale = request.ExchangeSale,
                Is_default = request.IsDefault,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Symbol = string.Empty,
                Code_iso = string.Empty,
                Exchange_sale = string.Empty,
                Is_default = 1,
                Updated_at = DateTime.MinValue,
                Created_at = DateTime.MinValue,
            });

            return new CreateCurrencyResponse
            {
                Id = result.Id,
                Description = result.Description,
                Symbol = result.Symbol,
                IsoCode = result.Code_iso,
                ExchangeRate = decimal.Parse(result.Exchange_sale),
                IsDefault = result.Is_default == 1,
                CreatedAt = result.Created_at,
                UpdatedAt = result.Updated_at,
            };
        }
        #endregion


        #region Payment Methods
        public async Task<SearchPaymentMethodResponse> SearchPaymentMethod(string apiKey, SearchPaymentMethodRequest request)
        {
            var result = await Post("api/v2/administration/paymentmethods/find", apiKey, new
            {
                Value = request.Value,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Description_english = (string?)null,
                Updated_at = DateTime.MinValue,
                Created_at = DateTime.MinValue,
            });

            return new SearchPaymentMethodResponse
            {
                Id = result.Id,
                Description = result.Description,
                EnglishDescription = result.Description_english,
                CreatedAt = result.Created_at,
                UpdatedAt = result.Updated_at,
            };
        }

        public async Task<CreatePaymentMethodResponse> CreatePaymentMethod(string apiKey, CreatePaymentMethodRequest request)
        {
            var result = await Post("api/v2/administration/paymentmethods", apiKey, new
            {
                Description = request.Description,
                Description_english = request.EnglishDescription,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Description_english = (string?)null,
                Updated_at = DateTime.MinValue,
                Created_at = DateTime.MinValue,
            });

            return new CreatePaymentMethodResponse
            {
                Id = result.Id,
                Description = result.Description,
                EnglishDescription = result.Description_english,
                CreatedAt = result.Created_at,
                UpdatedAt = result.Updated_at,
            };
        }
        #endregion

        #region Series
        public async Task<SearchSerieResponse> SearchSerie(string apiKey, SearchSerieRequest request)
        {
            var result = await Post("api/v2/administration/series/find", apiKey, new
            {
                Value = request.Value,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Valid_until = string.Empty,
                Updated_at = DateTime.MinValue,
                Created_at = DateTime.MinValue,
            });

            return new SearchSerieResponse
            {
                Id = result.Id,
                Description = result.Description,
                ValidUntilYear = long.Parse(result.Valid_until),
                CreatedAt = result.Created_at,
                UpdatedAt = result.Updated_at,
            };
        }

        public async Task<CreateSerieResponse> CreateSerie(string apiKey, CreateSerieRequest request)
        {
            var result = await Post("api/v2/administration/series", apiKey, new
            {
                Description = request.Description,
                Valid_until = request.ValidUntilYear,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Valid_until = string.Empty,
                Updated_at = DateTime.MinValue,
                Created_at = DateTime.MinValue,
            });

            return new CreateSerieResponse
            {
                Id = result.Id,
                Description = result.Description,
                ValidUntilYear = long.Parse(result.Valid_until),
                CreatedAt = result.Created_at,
                UpdatedAt = result.Updated_at,
            };
        }

        public async Task<CheckSerieCommunicationResponse> CheckSerieCommunication(string apiKey, CheckSerieCommunicationRequest request)
        {
            var result = await Post($"v2/administration/series/{request.Id}/check_communication", apiKey, new
            {
                Document_type = request.DocumentType,
            }, false);

            return new CheckSerieCommunicationResponse
            {
                Status = result,
            };
        }

        public async Task<CommunicateSerieResponse> CommunicateSerie(string apiKey, CommunicateSerieRequest request)
        {
            var result = await Post($"v2/administration/series/{request.Id}/communicate ", apiKey, new
            {
            }, new
            {
            });

            return new CommunicateSerieResponse
            {
            };
        }
        #endregion

        #region Payment Condition
        public async Task<SearchPaymentConditionResponse> SearchPaymentCondition(string apiKey, SearchPaymentConditionRequest request)
        {
            var result = await Post("api/v2/administration/paymentconditions/find", apiKey, new
            {
                Value = request.Value,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Description_english = (string?)null,
                Days = 0,
                Updated_at = DateTime.MinValue,
                Created_at = DateTime.MinValue,
            });

            return new SearchPaymentConditionResponse
            {
                Id = result.Id,
                Description = result.Description,
                EnglishDescription = result.Description_english,
                Days = result.Days,
                CreatedAt = result.Created_at,
                UpdatedAt = result.Updated_at,
            };
        }

        public async Task<CreatePaymentConditionResponse> CreatePaymentCondition(string apiKey, CreatePaymentConditionRequest request)
        {
            var result = await Post("api/v2/administration/paymentconditions", apiKey, new
            {
                Description = request.Description,
                Description_english = request.EnglishDescription,
                Days = request.Days,
            }, new
            {
                Id = 0L,
                Description = string.Empty,
                Description_english = (string?)null,
                Days = 0,
                Updated_at = DateTime.MinValue,
                Created_at = DateTime.MinValue,
            });

            return new CreatePaymentConditionResponse
            {
                Id = result.Id,
                Description = result.Description,
                EnglishDescription = result.Description_english,
                Days = result.Days,
                CreatedAt = result.Created_at,
                UpdatedAt = result.Updated_at,
            };
        }
        #endregion

        #region Customers
        public async Task<SearchCustomerResponse> SearchCustomer(string apiKey, SearchCustomerRequest request)
        {
            var result = await Post("api/v2/customers/find", apiKey, new
            {
                Value = request.Value,
                Search_in = request.SearchField,
            }, new
            {
                Id = 0L,
                Code = string.Empty,
                Name = string.Empty,
                Vat_number = (string?)null,
                Country = (string?)null,
                Address = (string?)null,
                City = (string?)null,
                Postal_code = (string?)null,
                Email = (string?)null,
                Telephone = (string?)null,
                Mobile = (string?)null,
                Currency_id = (long?)null,
                Payment_method_id = (long?)null,
                Payment_condition_id = (long?)null,
                Shipping_mode_id = (long?)null,
                Price_id = (long?)null,
                Employee_id = (long?)null,
                Type = CustomerType.Business,
                Vat_exemption_id = (long?)null,
                Vat_type = VatType.VatDebit,
                Irs_retention_tax = string.Empty,
                Observations = (string?)null,
                Other_contacts = (string?)null,
                Other_emails = (string?)null,
                Receive_sms = 1,
                Receive_emails = 1,
                Language = (string?)null,
            });

            return new SearchCustomerResponse
            {
                Id = result.Id,
                Code = result.Code,
                Name = result.Name,
                VatNumber = result.Vat_number,
                Country = result.Country,
                Address = result.Address,
                City = result.City,
                PostalCode = result.Postal_code,
                Email = result.Email,
                Phone = result.Telephone,
                MobilePhone = result.Mobile,
                CurrencyId = result.Currency_id,
                PaymentMethodId = result.Payment_method_id,
                PaymentConditionId = result.Payment_condition_id,
                ShippingModeId = result.Shipping_mode_id,
                PriceId = result.Price_id,
                EmployeeId = result.Employee_id,
                Type = result.Type,
                VatType = result.Vat_type,
                VatExemptionId = result.Vat_exemption_id,
                IrsRetentionTax = decimal.Parse(result.Irs_retention_tax),
                Observations = result.Observations,
                OtherContacts = result.Other_contacts,
                OtherEmails = result.Other_emails?.Split(","),
                ReceiveSms = result.Receive_sms == 1,
                ReceiveEmails = result.Receive_emails == 1,
                Language = result.Language,
            };
        }

        public async Task<CreateCustomerResponse> CreateCustomer(string apiKey, CreateCustomerRequest request)
        {
            var result = await Post("api/v2/customers", apiKey, new
            {
                Code = request.Code,
                Name = request.Name,
                Vat_number = request.VatNumber,
                Country = request.Country,
                Address = request.Address,
                City = request.City,
                Postal_code = request.PostalCode,
                Email = request.Email,
                Telephone = request.Phone,
                Mobile = request.MobilePhone,
                Currency = request.CurrencyId,
                Payment_method = request.PaymentMethodId,
                Payment_condition = request.PaymentConditionId,
                Shipping_mode = request.ShippingModeId,
                Price = request.PriceId,
                Employee = request.EmployeeId,
                Type = request.Type,
                Vat_type = request.VatType,
                Vat_exemption = request.VatExemptionId,
                Irs_retention_tax = request.IrsRetentionTax,
                Observations = request.Observations,
                Other_contacts = request.OtherContacts,
                Other_emails = request.OtherEmails == null ? null : string.Join(",", request.OtherEmails),
                Receive_sms = request.ReceiveSms,
                Receive_emails = request.ReceiveEmails,
                Language = request.Language,
                Addresses = request.Addresses?.Select(s => new
                {
                    Country = request.Country,
                    Address = s.Address,
                    City = s.City,
                    Postal_code = s.PostalCode,
                }),
            }, new
            {
                Id = 0L,
                Code = string.Empty,
                Name = string.Empty,
                Vat_number = (string?)null,
                Country = (string?)null,
                Address = (string?)null,
                City = (string?)null,
                Postal_code = (string?)null,
                Email = (string?)null,
                Telephone = (string?)null,
                Mobile = (string?)null,
                Currency_id = (long?)null,
                Payment_method_id = (long?)null,
                Payment_condition_id = (long?)null,
                Shipping_mode_id = (long?)null,
                Price_id = (long?)null,
                Employee_id = (long?)null,
                Type = CustomerType.Business,
                Vat_exemption_id = (long?)null,
                Vat_type = VatType.VatDebit,
                Irs_retention_tax = string.Empty,
                Observations = (string?)null,
                Other_contacts = (string?)null,
                Other_emails = (string?)null,
                Receive_sms = 1,
                Receive_emails = 1,
                Language = (string?)null,
            });

            return new CreateCustomerResponse
            {
                Id = result.Id,
                Code = result.Code,
                Name = result.Name,
                VatNumber = result.Vat_number,
                Country = result.Country,
                Address = result.Address,
                City = result.City,
                PostalCode = result.Postal_code,
                Email = result.Email,
                Phone = result.Telephone,
                MobilePhone = result.Mobile,
                CurrencyId = result.Currency_id,
                PaymentMethodId = result.Payment_method_id,
                PaymentConditionId = result.Payment_condition_id,
                ShippingModeId = result.Shipping_mode_id,
                PriceId = result.Price_id,
                EmployeeId = result.Employee_id,
                Type = result.Type,
                VatType = result.Vat_type,
                VatExemptionId = result.Vat_exemption_id,
                IrsRetentionTax = decimal.Parse(result.Irs_retention_tax),
                Observations = result.Observations,
                OtherContacts = result.Other_contacts,
                OtherEmails = result.Other_emails?.Split(","),
                ReceiveSms = result.Receive_sms == 1,
                ReceiveEmails = result.Receive_emails == 1,
                Language = result.Language,
            };
        }
        #endregion

        #region Sales
        public async Task<SearchSaleResponse> SearchSale(string apiKey, SearchSaleRequest request)
        {
            var result = await Post("api/v2/sales/find", apiKey, new
            {
                Value = request.Value,
                Search_in = request.Field,
            }, new
            {
                Id = 0L,
                Sale_reference_id = (long?)null,
                Issue_date = DateTime.MinValue,
                Due_date = (DateTime?)null,
                Serie_id = 0L,
                Document_type_id = 0L,
                Document_full_number = string.Empty,
                Gross_total = string.Empty,
                Total_discount = string.Empty,
                Net_total = string.Empty,
                Total_base_vat = string.Empty,
                Total_vat = string.Empty,
                Total_shipping = string.Empty,
                Grand_total = string.Empty,
                Grand_total_with_currency_exchange = string.Empty,
                Final_discount_financial = string.Empty,
                Final_discount_global = string.Empty,
                Final_discount_global_value = string.Empty,

                Customer_id = 0L,
                Customer_code = (string?)null,
                Customer_name = (string?)null,
                Customer_vat_number = (string?)null,
                Customer_country = (string?)null,
                Customer_city = (string?)null,
                Customer_address = (string?)null,
                Customer_postal_code = (string?)null,
                Customer_delivery_address_country = (string?)null,
                Customer_delivery_address_city = (string?)null,
                Customer_delivery_address_address = (string?)null,
                Customer_delivery_address_postal_code = (string?)null,

                Company_name = (string?)null,
                Company_vat_number = (string?)null,
                Company_country = (string?)null,
                Company_city = (string?)null,
                Company_address = (string?)null,
                Company_postal_code = (string?)null,

                Payment_method_id = (long?)null,
                Payment_condition_id = (long?)null,
                Shipping_mode_id = (long?)null,
                Shipping_value = (string?)null,
                Shipping_vat_id = (long?)null,
                Price_id = (long?)null,
                Currency_id = 0L,
                Currency_exchange = string.Empty,
                Vat_type = VatType.VatIncluded,
                Observations = (string?)null,

                Irs_retention_apply = string.Empty,
                Irs_retention_base = string.Empty,
                Irs_retention_total = string.Empty,
                Irs_retention_tax = string.Empty,

                Url_file = string.Empty,
                File_format = DocumentFormat.A4,

                Email_sent = 0,
                Sms_sent = 0,
                Status = SaleStatus.Draft,

                Vehicle_id = (long?)null,
                Employee_id = (long?)null,
                Waybill_shipping_date = (DateTime?)null,
                Waybill_global = (int?)null,

                At_code = string.Empty,
                At_message = string.Empty,

                Canceled_by = (long?)null,
                Canceled_at = (string?)null,
                Canceled_reason = (string?)null,

                Items = Enumerable.Repeat(new
                {
                    Id = 0L,
                    Item_id = 0L,
                    Item_details = (string?)null,
                    Unit_price = string.Empty,
                    Quantity = string.Empty,
                    Discount = string.Empty,
                    Gross_total = string.Empty,
                    Net_total = string.Empty,
                    Total_base_vat = string.Empty,
                    Total_vat = string.Empty,
                    Total_discount = string.Empty,
                    Grand_total = string.Empty,
                    Vat_id = 0L,
                    Unit_id = 0L,
                    Vat_exemption_id = 0L,
                    Item = new
                    {
                        Id = 0L,
                        Reference = string.Empty,
                        Description = string.Empty,
                        Details = (string?)null,
                        Type = ItemType.Service,
                    },
                    Unit = new
                    {
                        Id = 0L,
                        Description = string.Empty,
                        Symbol = string.Empty,
                    },
                    Vat = new
                    {
                        Id = 0L,
                        Description = string.Empty,
                        Type = TaxType.Isenta,
                        Saft_region = SaftRegion.PortugalContinental,
                        Tax = string.Empty,
                    },
                    Vatexemption = new
                    {
                        Id = 0L,
                        Code = string.Empty,
                        Description = string.Empty,
                    },
                }, 1),

                Serie = new
                {
                    Id = 0L,
                    Description = string.Empty,
                    Valid_until = string.Empty,
                },

                Paymentmethod = new
                {
                    Id = 0L,
                    Description = string.Empty,
                    Description_english = (string?)null,
                },

                Customer = new
                {
                    Id = 0L,
                    Code = string.Empty,
                    Name = string.Empty,
                    Vat_number = (string?)null,
                    Country = (string?)null,
                    Address = (string?)null,
                    City = (string?)null,
                    Postal_code = (string?)null,
                    Email = (string?)null,
                    Telephone = (string?)null,
                    Mobile = (string?)null,
                    Currency_id = (long?)null,
                    Payment_method_id = (long?)null,
                    Payment_condition_id = (long?)null,
                    Shipping_mode_id = (long?)null,
                    Price_id = (long?)null,
                    Employee_id = (long?)null,
                    Type = CustomerType.Business,
                    Vat_exemption_id = (long?)null,
                    Vat_type = VatType.VatDebit,
                    Irs_retention_tax = string.Empty,
                    Observations = (string?)null,
                    Other_contacts = (string?)null,
                    Other_emails = (string?)null,
                    Receive_sms = 1,
                    Receive_emails = 1,
                    Language = (string?)null,
                },

                Reference = new
                {
                    Id = 0L,
                    Number = 0L,
                    Issue_date = DateTime.MinValue,
                    Due_date = (DateTime?)null,
                    Serie_id = 0L,
                    Document_type_id = 0L,
                    Document_full_number = string.Empty,
                    Grand_total = string.Empty,
                    Serie = new
                    {
                        Id = 0L,
                        Description = string.Empty,
                    },
                }
            });

            return new SearchSaleResponse
            {
                Id = result.Id,
                SaleReferenceId = result.Sale_reference_id,
                IssueDate = result.Issue_date,
                DueDate = result.Due_date,
                SerieId = result.Serie_id,
                DocumentTypeId = result.Document_type_id,
                DocumentFullNumber = result.Document_full_number,
                GrossTotal = decimal.Parse(result.Gross_total),
                TotalDiscount = decimal.Parse(result.Total_discount),
                NetTotal = decimal.Parse(result.Net_total),
                TotalBaseVat = decimal.Parse(result.Total_base_vat),
                TotalVat = decimal.Parse(result.Total_vat),
                TotalShipping = decimal.Parse(result.Total_shipping),
                GrandTotal = decimal.Parse(result.Grand_total),
                GrandTotalAtExchangeRate = decimal.Parse(result.Grand_total_with_currency_exchange),
                FinalFinancialDiscount = decimal.Parse(result.Final_discount_financial),
                FinalGlobalDiscount = decimal.Parse(result.Final_discount_global),
                FinalGlobalDiscountValue = decimal.Parse(result.Final_discount_global_value),

                CustomerId = result.Customer_id,
                CustomerCode = result.Customer_code,
                CustomerName = result.Customer_name,
                CustomerVatNumber = result.Customer_vat_number,
                CustomerCountry = result.Customer_country,
                CustomerCity = result.Customer_city,
                CustomerAddress = result.Customer_address,
                CustomerPostalCode = result.Customer_postal_code,
                CustomerDeliveryAddressCountry = result.Customer_delivery_address_country,
                CustomerDeliveryAddressCity = result.Customer_delivery_address_city,
                CustomerDeliveryAddressAddress = result.Customer_delivery_address_address,
                CustomerDeliveryAddressPostalCode = result.Customer_delivery_address_postal_code,

                CompanyName = result.Company_name,
                CompanyVatNumber = result.Company_vat_number,
                CompanyCountry = result.Company_country,
                CompanyCity = result.Company_city,
                CompanyAddress = result.Company_address,
                CompanyPostalCode = result.Company_postal_code,

                PaymentMethodId = result.Payment_method_id,
                PaymentConditionId = result.Payment_condition_id,
                ShippingModeId = result.Shipping_mode_id,
                ShippingValue = result.Shipping_value == null ? null : decimal.Parse(result.Shipping_value),
                ShippingVatId = result.Shipping_vat_id,
                PriceId = result.Price_id,
                CurrencyId = result.Currency_id,
                CurrencyExchangeRate = decimal.Parse(result.Currency_exchange),
                VatType = result.Vat_type,
                Observations = result.Observations,

                IrsRetentionApply = decimal.Parse(result.Irs_retention_apply),
                IrsRetentionBase = decimal.Parse(result.Irs_retention_base),
                IrsRetentionTax = decimal.Parse(result.Irs_retention_tax),
                IrsRetentionTotal = decimal.Parse(result.Irs_retention_total),

                UrlFile = result.Url_file,
                FileFormat = result.File_format,

                EmailSent = result.Email_sent == 1,
                SmsSent = result.Sms_sent == 1,
                Status = result.Status,

                VehicleId = result.Vehicle_id,
                EmployeeId = result.Employee_id,
                WaybillShippingDate = result.Waybill_shipping_date,
                WaybillGlobal = result.Waybill_global.HasValue ? result.Waybill_global.Value == 1 : null,

                ATCode = result.At_code,
                ATMessage = result.At_message,

                CanceledAt = string.IsNullOrWhiteSpace(result.Canceled_at) ? null : TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(result.Canceled_at, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None), LisbonTZ),
                CanceledReason = result.Canceled_reason,

                Items = result.Items.Select(s => new SaleItem
                {
                    Id = s.Id,
                    ItemId = s.Item_id,
                    ItemDetails = s.Item_details,
                    UnitPrice = decimal.Parse(s.Unit_price),
                    Quantity = decimal.Parse(s.Quantity),
                    Discount = decimal.Parse(s.Discount),
                    GrossTotal = decimal.Parse(s.Gross_total),
                    NetTotal = decimal.Parse(s.Net_total),
                    TotalBaseVat = decimal.Parse(s.Total_base_vat),
                    TotalVat = decimal.Parse(s.Total_vat),
                    TotalDiscount = decimal.Parse(s.Total_discount),
                    GrandTotal = decimal.Parse(s.Grand_total),
                    VatRateId = s.Vat_id,
                    UnitId = s.Unit_id,
                    VatExemptionId = s.Vat_exemption_id,
                    Item = new SaleItemItem
                    {
                        Id = s.Item.Id,
                        Reference = s.Item.Reference,
                        Description = s.Item.Description,
                        Details = s.Item.Details,
                        Type = s.Item.Type,
                    },
                    Unit = new SaleItemUnit
                    {
                        Id = s.Unit.Id,
                        Description = s.Unit.Description,
                        Symbol = s.Unit.Symbol,
                    },
                    VatRate = new SaleItemVatRate
                    {
                        Id = s.Vat.Id,
                        Description = s.Vat.Description,
                        TaxPercentage = decimal.Parse(s.Vat.Tax),
                        SaftRegion = s.Vat.Saft_region,
                        Type = s.Vat.Type,
                    },
                    VatExemption = new SaleItemVatExemption
                    {
                        Id = s.Vatexemption.Id,
                        Code = s.Vatexemption.Code,
                        Description = s.Vatexemption.Description,
                    },
                }),

                Serie = new SaleSerie
                {
                    Id = result.Serie.Id,
                    Description = result.Serie.Description,
                    ValidUntilYear = int.Parse(result.Serie.Valid_until),
                },

                PaymentMethod = result.Paymentmethod == null ? null : new SalePaymentMethod
                {
                    Id = result.Paymentmethod.Id,
                    Description = result.Paymentmethod.Description,
                    EnglishDescription = result.Paymentmethod.Description_english,
                },

                Customer = new SaleCustomer
                {
                    Id = result.Customer.Id,
                    Code = result.Customer.Code,
                    Name = result.Customer.Name,
                    VatNumber = result.Customer.Vat_number,
                    Country = result.Customer.Country,
                    Address = result.Customer.Address,
                    City = result.Customer.City,
                    PostalCode = result.Customer.Postal_code,
                    Email = result.Customer.Email,
                    Phone = result.Customer.Telephone,
                    MobilePhone = result.Customer.Mobile,
                    CurrencyId = result.Customer.Currency_id,
                    PaymentMethodId = result.Customer.Payment_method_id,
                    PaymentConditionId = result.Customer.Payment_condition_id,
                    ShippingModeId = result.Customer.Shipping_mode_id,
                    PriceId = result.Customer.Price_id,
                    EmployeeId = result.Customer.Employee_id,
                    Type = result.Customer.Type,
                    VatType = result.Customer.Vat_type,
                    VatExemptionId = result.Customer.Vat_exemption_id,
                    IrsRetentionTax = decimal.Parse(result.Customer.Irs_retention_tax),
                    Observations = result.Customer.Observations,
                    OtherContacts = result.Customer.Other_contacts,
                    OtherEmails = result.Customer.Other_emails?.Split(","),
                    ReceiveSms = result.Customer.Receive_sms == 1,
                    ReceiveEmails = result.Customer.Receive_emails == 1,
                    Language = result.Customer.Language,
                },

                SaleReference = result.Reference == null ? null : new SaleReference
                {
                    Id = result.Reference.Id,
                    IssueDate = result.Reference.Issue_date,
                    DueDate = result.Reference.Due_date,
                    SerieId = result.Reference.Serie_id,
                    DocumentTypeId = result.Reference.Document_type_id,
                    DocumentFullNumber = result.Reference.Document_full_number,
                    GrandTotal = decimal.Parse(result.Reference.Grand_total),
                    Serie = new SaleSerie
                    {
                        Id = result.Reference.Serie.Id,
                        Description = result.Reference.Serie.Description,
                    },
                },
            };
        }

        public async Task<CreateSaleResponse> CreateSale(string apiKey, CreateSaleRequest request)
        {
            var result = await Post("api/v2/sales", apiKey, new
            {
                Sale_reference_id = request.SaleReferenceId,
                Issue_date = request.IssueDate.ToString("yyyy-MM-dd"),
                Due_date = request.DueDate,
                Document_type = request.DocumentType,
                Serie = request.SerieId,
                Final_discount_financial = request.FinancialDiscountAmount,
                Final_discount_global = request.FinancialGlobalDiscountPercentage,
                Customer = request.CustomerId,
                Vat_number = request.VatNumber,

                Address = request.Address,
                City = request.City,
                Postal_code = request.PostalCode,
                Country = request.Country,

                Delivery_address_address = request.DeliveryAddressAddress,
                Delivery_address_city = request.DeliveryAddressCity,
                Delivery_address_postal_code = request.DeliveryAddressPostalCode,
                Delivery_address_country = request.DeliveryAddressCountry,

                Payment_method = request.PaymentMethodId,
                PaymentCondition = request.PaymentConditionId,
                Shipping_mode = request.ShippingModeId,
                Price = request.PriceId,
                Currency = request.CurrencyId,
                Currency_exchange = request.CurrencyExchangeRate,
                Vat_type = request.VatType,
                Observations = request.Observations,
                Irs_retention_tax = request.IrsRetentionTax,
                Vehicle = request.VehicleId,
                Employee = request.EmployeeId,
                Waybill_shipping_date = request.WaybillShippingDate,
                Waybill_global = request.WaybillGlobal,
                Location_origin = request.LocationOriginId,
                Location_destiny = request.LocationDestinyId,
                Cargo_location = request.CargoLocation,
                Discharge_location = request.DischargeLocation,
                Cargo_date = request.CargoDate,
                Discharge_date = request.DischargeDate,
                Items = request.Items.Select(item => new
                {
                    Id = item.Id,
                    Details = item.Details,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Discount = item.Discount,
                    Vat = item.VatRateId,
                    Vat_exemption = item.VatExemption,
                }),
                Language = request.Language,
                Format = request.Format,
                Paper_size = request.PaperSize,
                Paper_left_margin = request.PaperLeftMargin,
                Paper_right_margin = request.PaperRightMargin,
                Paper_top_margin = request.PaperTopMargin,
                Paper_bottom_margin = request.PaperBottomMargin,
                Force_print = request.ForcePrint,
                Force_send_email = request.ForceSendEmail,
                Force_send_sms = request.ForceSendSms,
                Force_sign = request.ForceSign,
                Callback_url = request.CallbackUrl,
                Status = request.Status,
                Reference = request.Reference,
            }, new
            {
                Id = 0L,
                Sale_reference_id = (long?)null,
                Issue_date = DateTime.MinValue,
                Due_date = (DateTime?)null,
                Serie_id = 0L,
                Document_type_id = 0L,
                Document_full_number = string.Empty,
                Gross_total = string.Empty,
                Total_discount = string.Empty,
                Net_total = string.Empty,
                Total_base_vat = string.Empty,
                Total_vat = string.Empty,
                Total_shipping = string.Empty,
                Grand_total = string.Empty,
                Grand_total_with_currency_exchange = string.Empty,
                Final_discount_financial = string.Empty,
                Final_discount_global = string.Empty,
                Final_discount_global_value = string.Empty,

                Customer_id = 0L,
                Customer_code = (string?)null,
                Customer_name = (string?)null,
                Customer_vat_number = (string?)null,
                Customer_country = (string?)null,
                Customer_city = (string?)null,
                Customer_address = (string?)null,
                Customer_postal_code = (string?)null,
                Customer_delivery_address_country = (string?)null,
                Customer_delivery_address_city = (string?)null,
                Customer_delivery_address_address = (string?)null,
                Customer_delivery_address_postal_code = (string?)null,

                Company_name = (string?)null,
                Company_vat_number = (string?)null,
                Company_country = (string?)null,
                Company_city = (string?)null,
                Company_address = (string?)null,
                Company_postal_code = (string?)null,

                Payment_method_id = (long?)null,
                Payment_condition_id = (long?)null,
                Shipping_mode_id = (long?)null,
                Shipping_value = (string?)null,
                Shipping_vat_id = (long?)null,
                Price_id = (long?)null,
                Currency_id = 0L,
                Currency_exchange = string.Empty,
                Vat_type = VatType.VatIncluded,
                Observations = (string?)null,

                Irs_retention_apply = string.Empty,
                Irs_retention_base = string.Empty,
                Irs_retention_total = string.Empty,
                Irs_retention_tax = string.Empty,

                Url_file = string.Empty,
                File_format = DocumentFormat.A4,

                Email_sent = 0,
                Sms_sent = 0,
                Status = SaleStatus.Draft,

                Vehicle_id = (long?)null,
                Employee_id = (long?)null,
                Waybill_shipping_date = (DateTime?)null,
                Waybill_global = (int?)null,

                At_code = string.Empty,
                At_message = string.Empty,

                Canceled_by = (long?)null,
                Canceled_at = (string?)null,
                Canceled_reason = (string?)null,

                Items = Enumerable.Repeat(new
                {
                    Id = 0L,
                    Item_id = 0L,
                    Item_details = (string?)null,
                    Unit_price = string.Empty,
                    Quantity = string.Empty,
                    Discount = string.Empty,
                    Gross_total = string.Empty,
                    Net_total = string.Empty,
                    Total_base_vat = string.Empty,
                    Total_vat = string.Empty,
                    Total_discount = string.Empty,
                    Grand_total = string.Empty,
                    Vat_id = 0L,
                    Unit_id = 0L,
                    Vat_exemption_id = 0L,
                    Item = new
                    {
                        Id = 0L,
                        Reference = string.Empty,
                        Description = string.Empty,
                        Details = (string?)null,
                        Type = ItemType.Service,
                    },
                    Unit = new
                    {
                        Id = 0L,
                        Description = string.Empty,
                        Symbol = string.Empty,
                    },
                    Vat = new
                    {
                        Id = 0L,
                        Description = string.Empty,
                        Type = TaxType.Isenta,
                        Saft_region = SaftRegion.PortugalContinental,
                        Tax = string.Empty,
                    },
                    Vatexemption = new
                    {
                        Id = 0L,
                        Code = string.Empty,
                        Description = string.Empty,
                    },
                }, 1),

                Serie = new
                {
                    Id = 0L,
                    Description = string.Empty,
                    Valid_until = string.Empty,
                },

                Paymentmethod = new
                {
                    Id = 0L,
                    Description = string.Empty,
                    Description_english = (string?)null,
                },

                Customer = new
                {
                    Id = 0L,
                    Code = string.Empty,
                    Name = string.Empty,
                    Vat_number = (string?)null,
                    Country = (string?)null,
                    Address = (string?)null,
                    City = (string?)null,
                    Postal_code = (string?)null,
                    Email = (string?)null,
                    Telephone = (string?)null,
                    Mobile = (string?)null,
                    Currency_id = (long?)null,
                    Payment_method_id = (long?)null,
                    Payment_condition_id = (long?)null,
                    Shipping_mode_id = (long?)null,
                    Price_id = (long?)null,
                    Employee_id = (long?)null,
                    Type = CustomerType.Business,
                    Vat_exemption_id = (long?)null,
                    Vat_type = VatType.VatDebit,
                    Irs_retention_tax = string.Empty,
                    Observations = (string?)null,
                    Other_contacts = (string?)null,
                    Other_emails = (string?)null,
                    Receive_sms = 1,
                    Receive_emails = 1,
                    Language = (string?)null,
                },

                Reference = new
                {
                    Id = 0L,
                    Number = 0L,
                    Issue_date = DateTime.MinValue,
                    Due_date = (DateTime?)null,
                    Serie_id = 0L,
                    Document_type_id = 0L,
                    Document_full_number = string.Empty,
                    Grand_total = string.Empty,
                    Serie = new
                    {
                        Id = 0L,
                        Description = string.Empty,
                    },
                }
            });

            return new CreateSaleResponse
            {
                Id = result.Id,
                SaleReferenceId = result.Sale_reference_id,
                IssueDate = result.Issue_date,
                DueDate = result.Due_date,
                SerieId = result.Serie_id,
                DocumentTypeId = result.Document_type_id,
                DocumentFullNumber = result.Document_full_number,
                GrossTotal = decimal.Parse(result.Gross_total),
                TotalDiscount = decimal.Parse(result.Total_discount),
                NetTotal = decimal.Parse(result.Net_total),
                TotalBaseVat = decimal.Parse(result.Total_base_vat),
                TotalVat = decimal.Parse(result.Total_vat),
                TotalShipping = decimal.Parse(result.Total_shipping),
                GrandTotal = decimal.Parse(result.Grand_total),
                GrandTotalAtExchangeRate = decimal.Parse(result.Grand_total_with_currency_exchange),
                FinalFinancialDiscount = decimal.Parse(result.Final_discount_financial),
                FinalGlobalDiscount = decimal.Parse(result.Final_discount_global),
                FinalGlobalDiscountValue = decimal.Parse(result.Final_discount_global_value),

                CustomerId = result.Customer_id,
                CustomerCode = result.Customer_code,
                CustomerName = result.Customer_name,
                CustomerVatNumber = result.Customer_vat_number,
                CustomerCountry = result.Customer_country,
                CustomerCity = result.Customer_city,
                CustomerAddress = result.Customer_address,
                CustomerPostalCode = result.Customer_postal_code,
                CustomerDeliveryAddressCountry = result.Customer_delivery_address_country,
                CustomerDeliveryAddressCity = result.Customer_delivery_address_city,
                CustomerDeliveryAddressAddress = result.Customer_delivery_address_address,
                CustomerDeliveryAddressPostalCode = result.Customer_delivery_address_postal_code,

                CompanyName = result.Company_name,
                CompanyVatNumber = result.Company_vat_number,
                CompanyCountry = result.Company_country,
                CompanyCity = result.Company_city,
                CompanyAddress = result.Company_address,
                CompanyPostalCode = result.Company_postal_code,

                PaymentMethodId = result.Payment_method_id,
                PaymentConditionId = result.Payment_condition_id,
                ShippingModeId = result.Shipping_mode_id,
                ShippingValue = result.Shipping_value == null ? null : decimal.Parse(result.Shipping_value),
                ShippingVatId = result.Shipping_vat_id,
                PriceId = result.Price_id,
                CurrencyId = result.Currency_id,
                CurrencyExchangeRate = decimal.Parse(result.Currency_exchange),
                VatType = result.Vat_type,
                Observations = result.Observations,

                IrsRetentionApply = decimal.Parse(result.Irs_retention_apply),
                IrsRetentionBase = decimal.Parse(result.Irs_retention_base),
                IrsRetentionTax = decimal.Parse(result.Irs_retention_tax),
                IrsRetentionTotal = decimal.Parse(result.Irs_retention_total),

                UrlFile = result.Url_file,
                FileFormat = result.File_format,

                EmailSent = result.Email_sent == 1,
                SmsSent = result.Sms_sent == 1,
                Status = result.Status,

                VehicleId = result.Vehicle_id,
                EmployeeId = result.Employee_id,
                WaybillShippingDate = result.Waybill_shipping_date,
                WaybillGlobal = result.Waybill_global.HasValue ? result.Waybill_global.Value == 1 : null,

                ATCode = result.At_code,
                ATMessage = result.At_message,

                CanceledAt = string.IsNullOrWhiteSpace(result.Canceled_at) ? null : TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(result.Canceled_at, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None), LisbonTZ),
                CanceledReason = result.Canceled_reason,

                Items = result.Items.Select(s => new SaleItem
                {
                    Id = s.Id,
                    ItemId = s.Item_id,
                    ItemDetails = s.Item_details,
                    UnitPrice = decimal.Parse(s.Unit_price),
                    Quantity = decimal.Parse(s.Quantity),
                    Discount = decimal.Parse(s.Discount),
                    GrossTotal = decimal.Parse(s.Gross_total),
                    NetTotal = decimal.Parse(s.Net_total),
                    TotalBaseVat = decimal.Parse(s.Total_base_vat),
                    TotalVat = decimal.Parse(s.Total_vat),
                    TotalDiscount = decimal.Parse(s.Total_discount),
                    GrandTotal = decimal.Parse(s.Grand_total),
                    VatRateId = s.Vat_id,
                    UnitId = s.Unit_id,
                    VatExemptionId = s.Vat_exemption_id,
                    Item = new SaleItemItem
                    {
                        Id = s.Item.Id,
                        Reference = s.Item.Reference,
                        Description = s.Item.Description,
                        Details = s.Item.Details,
                        Type = s.Item.Type,
                    },
                    Unit = new SaleItemUnit
                    {
                        Id = s.Unit.Id,
                        Description = s.Unit.Description,
                        Symbol = s.Unit.Symbol,
                    },
                    VatRate = new SaleItemVatRate
                    {
                        Id = s.Vat.Id,
                        Description = s.Vat.Description,
                        TaxPercentage = decimal.Parse(s.Vat.Tax),
                        SaftRegion = s.Vat.Saft_region,
                        Type = s.Vat.Type,
                    },
                    VatExemption = new SaleItemVatExemption
                    {
                        Id = s.Vatexemption.Id,
                        Code = s.Vatexemption.Code,
                        Description = s.Vatexemption.Description,
                    },
                }),

                Serie = new SaleSerie
                {
                    Id = result.Serie.Id,
                    Description = result.Serie.Description,
                    ValidUntilYear = int.Parse(result.Serie.Valid_until),
                },

                PaymentMethod = result.Paymentmethod == null ? null : new SalePaymentMethod
                {
                    Id = result.Paymentmethod.Id,
                    Description = result.Paymentmethod.Description,
                    EnglishDescription = result.Paymentmethod.Description_english,
                },

                Customer = new SaleCustomer
                {
                    Id = result.Customer.Id,
                    Code = result.Customer.Code,
                    Name = result.Customer.Name,
                    VatNumber = result.Customer.Vat_number,
                    Country = result.Customer.Country,
                    Address = result.Customer.Address,
                    City = result.Customer.City,
                    PostalCode = result.Customer.Postal_code,
                    Email = result.Customer.Email,
                    Phone = result.Customer.Telephone,
                    MobilePhone = result.Customer.Mobile,
                    CurrencyId = result.Customer.Currency_id,
                    PaymentMethodId = result.Customer.Payment_method_id,
                    PaymentConditionId = result.Customer.Payment_condition_id,
                    ShippingModeId = result.Customer.Shipping_mode_id,
                    PriceId = result.Customer.Price_id,
                    EmployeeId = result.Customer.Employee_id,
                    Type = result.Customer.Type,
                    VatType = result.Customer.Vat_type,
                    VatExemptionId = result.Customer.Vat_exemption_id,
                    IrsRetentionTax = decimal.Parse(result.Customer.Irs_retention_tax),
                    Observations = result.Customer.Observations,
                    OtherContacts = result.Customer.Other_contacts,
                    OtherEmails = result.Customer.Other_emails?.Split(","),
                    ReceiveSms = result.Customer.Receive_sms == 1,
                    ReceiveEmails = result.Customer.Receive_emails == 1,
                    Language = result.Customer.Language,
                },

                SaleReference = result.Reference == null ? null : new SaleReference
                {
                    Id = result.Reference.Id,
                    IssueDate = result.Reference.Issue_date,
                    DueDate = result.Reference.Due_date,
                    SerieId = result.Reference.Serie_id,
                    DocumentTypeId = result.Reference.Document_type_id,
                    DocumentFullNumber = result.Reference.Document_full_number,
                    GrandTotal = decimal.Parse(result.Reference.Grand_total),
                    Serie = new SaleSerie
                    {
                        Id = result.Reference.Serie.Id,
                        Description = result.Reference.Serie.Description,
                    },
                },
            };
        }

        public async Task<DownloadSaleFileResponse> DownloadSaleFile(string apiKey, DownloadSaleFileRequest request)
        {
            var result = await Post<string>($"api/v2/sales/{request.SaleId}/download", apiKey, new
            {
                Language = request.Language,
                Format = request.Format,
                Paper_size = request.PaperSize,

                Paper_left_margin = request.PaperLeftMargin,
                Paper_right_margin = request.PaperRightMargin,
                Paper_top_margin = request.PaperTopMargin,
                Paper_bottom_margin = request.PaperBottomMargin,
                Issue = request.Issue,
                Max_items_per_page = request.MaxItemsPerPage,
            });

            return new DownloadSaleFileResponse
            {
                Url = result,
            };
        }

        public async Task<CancelSaleResponse> CancelSale(string apiKey, CancelSaleRequest request)
        {
            var result = await Put<string>($"api/v2/sales/{request.SaleId}/cancel", apiKey, new
            {
                Reason = request.Reason,
            });

            return new CancelSaleResponse
            {
                Url = result,
            };
        }
        #endregion
    }
}