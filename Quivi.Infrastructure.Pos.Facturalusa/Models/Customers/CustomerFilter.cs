using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Customers
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<CustomerFilter>))]
    public enum CustomerFilter
    {
        [JsonProperty("ID")]
        Id,

        [JsonProperty("Code")]
        Code,

        [JsonProperty("Name")]
        Name,

        [JsonProperty("Email")]
        Email,

        [JsonProperty("Vat Number")]
        VatNumber,

        [JsonProperty("Mobile")]
        MobilePhone,
    }
}
