using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Items
{
    public class CreateItemResponse : AResponseBase<ReadonlyItem>
    {

    }

    public class UpdateItemResponse : AResponseBase
    {
        /// <summary>
        /// Indicates if item was updated successfully.
        /// </summary>
        [JsonProperty("status")]
        public bool Success { get; set; }
    }
}
