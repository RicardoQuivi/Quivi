using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.JsonConverters;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public class DownloadSaleRequest : ARequestBase
    {
        [JsonProperty("format")]
        public DownloadSaleFormat Format { get; set; }

        [JsonProperty("paper_size")]
        public int DocumentWidthMilimeters { get; set; }

        [JsonProperty("issue")]
        public DownloadSaleIssue Issue { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter<DownloadSaleFormat>))]
    public enum DownloadSaleFormat
    {
        [JsonProperty("A4")]
        A4 = 0,

        [JsonProperty("POS")]
        POS,

        [JsonProperty("EscPOS")]
        EscPOS,
    }
    
    [JsonConverter(typeof(JsonStringEnumMemberConverter<DownloadSaleIssue>))]
    public enum DownloadSaleIssue
    {
        [JsonProperty("2ª via")]
        SecondCopy,

        [JsonProperty("Original")]
        Original,
    }
}
