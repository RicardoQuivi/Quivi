using Quivi.Domain.Entities.Financing;

namespace Quivi.Domain.Entities.Identity
{
    public class ApiClient : IDeletableEntity
    {
        public int ApiClientId { get; set; }

        public string? UserName { get; set; }
        public string? Password { get; set; }
        public BasicAuthClientType ClientType { get; set; }
        public DateTime? LastActivity { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int? UserId { get; set; }

        public int? PersonId { get; set; }
        public Person? Person { get; set; }

        public ICollection<ApiClientRequest>? ApiClientRequests { get; set; }
        #endregion
    }
}
