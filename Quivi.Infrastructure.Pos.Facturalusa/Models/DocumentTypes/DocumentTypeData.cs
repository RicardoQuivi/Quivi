using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.DocumentTypes
{
    public class DocumentTypeData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("description")]
        public DocumentType Type { get; set; }

        [JsonProperty("saft_initials")]
        public string? Code { get; set; }
    }
}
