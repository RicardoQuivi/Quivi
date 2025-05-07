using Microsoft.AspNetCore.Identity;
using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Identity;
using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Repositories.EntityFramework.Identity
{
    public class ApplicationUser : IdentityUser<int>, IEntity
    {
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public ICollection<ApiClient>? ApiClients { get; set; }
        public ICollection<Merchant>? Merchants { get; set; }
        #endregion
    }
}
