using Quivi.Domain.Repositories.EntityFramework.Identity;

namespace Quivi.OAuth2.Handlers
{
    public class NoPrincipalUserContext : IUserContext
    {
        public int Id => user.Id;
        public string Email => user.Email!;
        public string? FullName => user.FullName;
        public IEnumerable<string> Roles { get; }


        private readonly ApplicationUser user;
        public NoPrincipalUserContext(ApplicationUser user, IEnumerable<string> roles)
        {
            this.user = user;
            Roles = roles;
        }
    }
}