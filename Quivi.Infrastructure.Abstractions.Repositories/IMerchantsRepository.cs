using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface IMerchantsRepository : IRepository<Merchant, GetMerchantsCriteria>
    {
    }
}
