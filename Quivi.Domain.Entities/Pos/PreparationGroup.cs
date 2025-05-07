using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class PreparationGroup : IEntity
    {
        public int Id { get; set; }
        public string? AdditionalNote { get; set; }
        public PreparationGroupState State { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public int SessionId { get; set; }
        public Session? Session { get; set; }

        public int? ParentPreparationGroupId { get; set; }
        public PreparationGroup? ParentPreparationGroup { get; set; }
        public ICollection<PreparationGroup>? ChildrenPreparationGroups { get; set; }

        public ICollection<PreparationGroupItem>? PreparationGroupItems { get; set; }

        public ICollection<Order>? Orders { get; set; }
        #endregion
    }
}