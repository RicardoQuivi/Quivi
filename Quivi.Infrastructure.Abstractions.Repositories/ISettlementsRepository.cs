using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface ISettlementsRepository : IRepository<Settlement, GetSettlementsCriteria>
    {
        Task<IPagedData<MerchantSettlementResume>> GetMerchantSettlementResumes(GetMerchantSettlementResumesCriteria criteria);
        Task<IPagedData<MerchantSettlementDetail>> GetMerchantSettlementDetails(GetMerchantSettlementDetailsCriteria criteria);
    }
}