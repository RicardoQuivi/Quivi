using Quivi.Domain.Repositories.EntityFramework.Identity;
using System.Security.Principal;

namespace Quivi.OAuth2.Handlers
{
    public class PrincipalUserContext : IUserContext
    {
        public int Id => user.Id;
        public string Email => user.Email!;
        public string? FullName => user.FullName;
        public IEnumerable<string> Roles { get; }
        public IPrincipal Principal { get; }

        private readonly ApplicationUser user;

        public PrincipalUserContext(ApplicationUser user, IEnumerable<string> roles, IPrincipal principal)
        {
            this.user = user;
            Roles = roles;
            Principal = principal;
        }
    }
}