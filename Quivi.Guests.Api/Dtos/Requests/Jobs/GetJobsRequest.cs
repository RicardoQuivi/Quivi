namespace Quivi.Guests.Api.Dtos.Requests.Jobs
{
    public class GetJobsRequest : ARequest
    {
        public required IEnumerable<string> Ids { get; init; }
    }
}