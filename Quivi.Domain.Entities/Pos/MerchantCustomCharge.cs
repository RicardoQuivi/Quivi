using Quivi.Domain.Entities.Charges;

namespace Quivi.Domain.Entities.Pos
{
    public class MerchantCustomCharge : IEntity
    {
        public int Id => ChargeId;

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int ChargeId { get; set; }
        public Charge? Charge { get; set; }

        public int CustomChargeMethodId { get; set; }
        public CustomChargeMethod? CustomChargeMethod { get; set; }
        #endregion
    }
}