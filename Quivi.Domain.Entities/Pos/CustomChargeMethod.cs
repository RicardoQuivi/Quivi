using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class CustomChargeMethod : IEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }
        public string? Logo { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }
        #endregion
    }
}
