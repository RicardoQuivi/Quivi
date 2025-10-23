using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface ISettlementServiceDetailsRepository : IRepository<SettlementServiceDetail, GetSettlementServiceDetailsCriteria>
    {
    }
}