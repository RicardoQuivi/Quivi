namespace Quivi.Backoffice.Api.Requests.Reviews
{
    public class GetReviewsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}
