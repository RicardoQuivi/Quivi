using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public class DocumentNumberData
    {
        [JsonProperty("document_number")]
        public int Number { get; set; }
    }
}
