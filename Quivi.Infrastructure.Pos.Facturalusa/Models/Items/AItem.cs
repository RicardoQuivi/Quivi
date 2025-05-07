using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Items
{
    public abstract class AItem
    {
        /// <summary>
        /// The Code that should be used when the item has no known category.
        /// </summary>
        public static readonly string DefaultCode = "Diversos";
        
        [JsonProperty("reference")]
        public string? Reference { get; set; }

        [JsonProperty("description")]
        public string? Name { get; set; }

        [JsonProperty("observations")]
        public string? JsonExtraInfo { get; set; }
        
        [JsonProperty("type")]
        public ItemType Type { get; set; }

        [JsonIgnore]
        public ItemExtraInfo? ExtraInfo 
        {
            get 
            {
                try
                {
                    return JsonConvert.DeserializeObject<ItemExtraInfo>(JsonExtraInfo!);
                }
                catch (System.Exception)
                {
                    return new ItemExtraInfo();
                }
            }
            set 
            {
                JsonExtraInfo = JsonConvert.SerializeObject(value);
            }
        }

        public class ItemExtraInfo
        {
            public string CorrelationId { get; set; }
        }
    }
}
