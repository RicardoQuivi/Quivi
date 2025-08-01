using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface IPostingsRepository : IRepository<Posting, GetPostingsCriteria>
    {
        Task<IReadOnlyDictionary<Person, decimal>> GetBalanceAsync(GetAccountBalanceCriteria criteria);
    }
}