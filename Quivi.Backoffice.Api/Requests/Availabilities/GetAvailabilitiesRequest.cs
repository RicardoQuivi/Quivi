namespace Quivi.Backoffice.Api.Requests.Availabilities
{
    public class GetAvailabilitiesRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}