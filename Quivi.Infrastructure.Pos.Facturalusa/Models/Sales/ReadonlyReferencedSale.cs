using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.Models.DocumentTypes;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Series;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public class ReadonlyReferencedSale
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }
        
        [JsonProperty("serie")]
        public ReadonlySerie? Serie { get; set; }
        
        [JsonProperty("documenttype")]
        public DocumentTypeData? DocumentTypeData { get; set; }

        [JsonIgnore]
        public string? ComposedNumber
        {
            get
            {
                if (string.IsNullOrEmpty(DocumentTypeData?.Code) || string.IsNullOrEmpty(Serie?.Name))
                    return null;

                return $"{DocumentTypeData.Code} {Serie.Name}/{Number}";
            }
        }
    }
}