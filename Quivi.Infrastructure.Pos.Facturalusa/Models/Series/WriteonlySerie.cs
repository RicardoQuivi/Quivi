using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Series
{
    public class WriteonlySerie : ASerie
    {
        [JsonProperty("assign_to_all")]
        public bool AssignToAllDocumentTypes { get; set; }
    }
}
