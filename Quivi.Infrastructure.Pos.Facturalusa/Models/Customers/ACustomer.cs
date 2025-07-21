using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Customers
{
    public abstract class ACustomer
    {
        /// <summary>
        /// The default code when there is no information about Customer.
        /// </summary>
        public static readonly string DefaultCode = "Consumidor Final";
        
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("vat_number")]
        public string? VatNumber { get; set; }

        [JsonProperty("mobile")]
        public string? MobileNumber { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("country")]
        public string? CountryName { get; set; }

        [JsonIgnore]
        public static string DefaultCountryName = "Portugal";

        [JsonProperty("city")]
        public string? CityName { get; set; }

        [JsonIgnore]
        public static string DefaultCityName = "Desconhecido";

        [JsonProperty("postal_code")]
        public string? PostalCode { get; set; }

        [JsonIgnore]
        public static string DefaultPostalCode = "0000-000";

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonIgnore]
        public static string DefaultAddress = "Desconhecido";

        [JsonProperty("type")]
        public CustomerType Type { get; set; }

        [JsonProperty("vat_type")]
        public VatRateType VatType { get; set; }

        [JsonProperty("language")]
        public Language Language { get; set; }
    }
}