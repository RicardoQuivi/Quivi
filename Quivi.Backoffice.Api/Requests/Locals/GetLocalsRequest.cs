namespace Quivi.Backoffice.Api.Requests.Locals
{
    public class GetLocalsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}
