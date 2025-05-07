using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models
{
    public abstract class AResponseBase : IResponse
    {
        
    }

    public abstract class AResponseBase<T> : AResponseBase where T : class
    {
        /// <summary>
        /// The response data.
        /// </summary>
        [JsonProperty("data")]
        public required T Data { get; init; }
    }
}