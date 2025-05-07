using Quivi.Domain.Entities.Identity;
using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Financing
{
    public class Person : IDeletableEntity
    {
        public int PersonId { get; set; }

        public string? PhoneNumber { get; set; }
        public Guid? SessionGuid { get; set; } //TODO: Can I delete this?
        public PersonType PersonType { get; set; } = PersonType.Consumer;
        public string? Vat { get; set; }
        public string? IdentityNumber { get; set; }

        public DateTime? ExpireDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int? MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public int? SubMerchantId { get; set; }
        public Merchant? SubMerchant { get; set; }

        public ICollection<ApiClient> ApiClients { get; set; }
        public ICollection<Posting> Postings { get; set; }
        public MerchantService? MerchantService { get; set; }
        #endregion
    }
}
