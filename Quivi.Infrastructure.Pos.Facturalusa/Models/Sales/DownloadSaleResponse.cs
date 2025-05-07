using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public class DownloadSaleResponse : AResponseBase 
    {
        [JsonProperty("url_file")]
        public required string FileUrl { get; set; }
    }
}
