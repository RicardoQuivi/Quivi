using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Items
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter<ItemType>))]
    public enum ItemType
    {
        [JsonProperty("Produtos acabados e intermédios")]
        FinishAndIntermediateProducts = 0,
        
        [JsonProperty("Embalagens")]
        Packages,

        [JsonProperty("Matérias primas")]
        RawMaterial,

        [JsonProperty("Mercadorias")]
        Commodity,

        [JsonProperty("Serviços")]
        Services,

        [JsonProperty("Subprodutos")]
        SubProducts,

        [JsonProperty("Transporte")]
        Transport,

        [JsonProperty("Vasilhame")]
        Container,
    }
}
