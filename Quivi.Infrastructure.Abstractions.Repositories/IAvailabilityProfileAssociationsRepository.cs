using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface IAvailabilityProfileAssociationsRepository : IRepository<AvailabilityProfileAssociation, GetAvailabilityProfileAssociationsCriteria>
    {
    }
}