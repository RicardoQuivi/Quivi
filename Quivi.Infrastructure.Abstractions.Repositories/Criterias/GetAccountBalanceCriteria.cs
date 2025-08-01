using Quivi.Domain.Entities.Financing;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetAccountBalanceCriteria
    {
        public IEnumerable<int>? PersonIds { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public string? PhoneNumber { get; init; }
        public IEnumerable<JournalType>? JournalTypes { get; init; }
    }
}