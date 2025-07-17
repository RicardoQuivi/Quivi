using Quivi.Infrastructure.Apis;

namespace Quivi.Guests.Api.Dtos.Requests.Reviews
{
    public class PatchReviewRequest : ARequest
    {
        public int? Stars { get; init; }
        public Optional<string?> Comment { get; init; }
    }
}