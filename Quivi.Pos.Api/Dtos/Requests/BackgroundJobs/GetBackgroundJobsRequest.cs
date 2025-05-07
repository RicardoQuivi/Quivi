namespace Quivi.Pos.Api.Dtos.Requests.BackgroundJobs
{
    public class GetBackgroundJobsRequest : ARequest
    {
        public required IEnumerable<string> Ids { get; init; }
    }
}