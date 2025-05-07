using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models
{
    public abstract class AGetRequestBase : ARequestBase
    {
        [JsonProperty("value")]
        public object? Value { get; set; }
    }

    public abstract class AGetRequestBase<T> : AGetRequestBase where T : Enum
    {
        [JsonProperty("search_in")]
        public T? FilterBy { get; set; }
    }
}
