using Quivi.Domain.Entities.Identity;
using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Financing
{
    public class Person : IDeletableEntity
    {
        public int Id { get; set; }

        public bool IsAnonymous { get; set; }
        public int? UserId { get; set; }
        public string? PhoneNumber { get; set; }
        public PersonType PersonType { get; set; } = PersonType.Consumer;
        public string? Vat { get; set; }
        public string? IdentityNumber { get; set; }

        public DateTime? ExpireDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int? ParentMerchantId { get; set; }
        public Merchant? ParentMerchant { get; set; }

        public int? MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public ICollection<ApiClient>? ApiClients { get; set; }
        public ICollection<Posting>? Postings { get; set; }
        public MerchantService? MerchantService { get; set; }
        #endregion
    }
}