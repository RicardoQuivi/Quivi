using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Customers
{
    public class ReadOnlyCustomer : ACustomer
    {
        [JsonProperty("id")]
        public long Id { get; set; }
    }
}