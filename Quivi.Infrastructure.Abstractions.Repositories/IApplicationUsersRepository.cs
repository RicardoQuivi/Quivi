using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface IApplicationUsersRepository : IRepository<ApplicationUser, GetApplicationUsersCriteria>
    {
    }
}
