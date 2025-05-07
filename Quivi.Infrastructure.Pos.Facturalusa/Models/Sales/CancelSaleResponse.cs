using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public class CancelSaleResponse
    {
        [JsonProperty("status")]
        public bool Status { get; set; }
        [JsonProperty("url_file")]
        public required string UrlFile { get; set; }
    }
}
