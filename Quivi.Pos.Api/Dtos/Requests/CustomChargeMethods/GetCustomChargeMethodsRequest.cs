namespace Quivi.Pos.Api.Dtos.Requests.CustomChargeMethods
{
    public class GetCustomChargeMethodsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}