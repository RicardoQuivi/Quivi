using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public class CancelSaleRequest : WriteonlySale, IRequest
    {
        [JsonProperty("format")]
        public DownloadSaleFormat Format { get; set; }

        [JsonProperty("force_print")]
        public bool IncludePdfFileUrl { get; set; }

        [JsonProperty("reason")]
        public string? Reason { get; set; }
    }
}
