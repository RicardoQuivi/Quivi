using Quivi.Domain.Entities.Charges;

namespace Quivi.Domain.Entities.Merchants
{
    public class MerchantFee : IDeletableEntity
    {
        public int MerchantId { get; set; }
        public required Merchant Merchant { get; set; }

        public ChargeMethod ChargeMethod { get; set; }
        public FeeType FeeType { get; set; }

        public decimal Fee { get; set; }
        public FeeUnit FeeUnit { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
