using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Items
{
    public class CreateItemRequest : WriteonlyItem, IRequest
    {

    }

    public class UpdateItemRequest : WriteonlyItem, IRequest 
    {
        [JsonProperty("active")]
        public bool IsActive { get; set; }
    }
}