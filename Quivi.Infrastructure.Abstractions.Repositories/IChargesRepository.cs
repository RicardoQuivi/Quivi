using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface IChargesRepository : IRepository<Charge, GetChargesCriteria>
    {
    }
}
