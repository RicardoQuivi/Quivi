using Quivi.Domain.Entities.Financing;

namespace Quivi.Domain.Entities.Merchants
{
    public class MerchantService : IEntity
    {
        public required string Name { get; set; }
        public MerchantServiceType Type { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int PersonId { get; set; }
        public required Person Person { get; set; }

        public int MerchantId { get; set; }
        public required Merchant Merchant { get; set; }
        #endregion
    }
}
