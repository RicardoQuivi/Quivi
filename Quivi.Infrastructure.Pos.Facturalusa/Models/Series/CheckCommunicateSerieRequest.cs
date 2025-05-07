using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.Models.DocumentTypes;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Series
{
    public class CheckCommunicateSerieRequest : ARequestBase 
    {
        [JsonProperty("document_type")]
        public DocumentType DocumentType { get; set; }
    }
}