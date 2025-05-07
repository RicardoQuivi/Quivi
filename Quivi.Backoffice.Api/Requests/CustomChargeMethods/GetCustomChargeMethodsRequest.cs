namespace Quivi.Backoffice.Api.Requests.CustomChargeMethods
{
    public class GetCustomChargeMethodsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}