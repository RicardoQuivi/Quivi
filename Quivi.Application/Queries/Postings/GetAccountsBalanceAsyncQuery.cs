using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Queries.Postings
{
    public class GetAccountsBalanceAsyncQuery : IQuery<Task<IReadOnlyDictionary<Person, decimal>>>
    {
        public IEnumerable<int>? PersonIds { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public string? PhoneNumber { get; init; }
        public IEnumerable<JournalType>? JournalTypes { get; init; }
    }

    public class GetAccountBalanceQueryHandler : IQueryHandler<GetAccountsBalanceAsyncQuery, Task<IReadOnlyDictionary<Person, decimal>>>
    {
        private readonly IPostingsRepository repository;

        public GetAccountBalanceQueryHandler(IPostingsRepository postingRepository)
        {
            repository = postingRepository;
        }

        public Task<IReadOnlyDictionary<Person, decimal>> Handle(GetAccountsBalanceAsyncQuery query)
        {
            return repository.GetBalanceAsync(new GetAccountBalanceCriteria
            {
                PersonIds = query.PersonIds,
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                PhoneNumber = query.PhoneNumber,
                JournalTypes = query.JournalTypes,
            });
        }
    }
}