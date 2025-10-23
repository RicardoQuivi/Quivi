using Quivi.Domain.Entities.Financing;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface ISettlementDetailsRepository : IRepository<SettlementDetail, GetSettlementDetailsCriteria>
    {
    }
}