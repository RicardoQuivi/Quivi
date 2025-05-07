using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Customers
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<CustomerType>))]
    public enum CustomerType
    {
        [JsonProperty("Particular")]
        Personal = 0,

        [JsonProperty("Empresarial")]
        Company,
    }
}
