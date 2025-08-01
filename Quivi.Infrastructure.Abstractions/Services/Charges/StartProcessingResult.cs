using Quivi.Domain.Entities.Charges;

namespace Quivi.Infrastructure.Abstractions.Services.Charges
{
    public class StartProcessingResult
    {
        public Charge? Charge { get; init; }
        public string? ChallengeUrl { get; init; }
    }
}