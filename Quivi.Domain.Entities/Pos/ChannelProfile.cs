using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class ChannelProfile : IDeletableEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }
        public decimal? PrePaidOrderingMinimumAmount { get; set; }
        public int? SendToPreparationTimerSeconds { get; set; }
        public ChannelFeature Features { get; set; } = ChannelFeature.AllowsOrderAndPay | ChannelFeature.AllowsPayAtTheTable;
        public TimeSpan? SendToPreparationTimer
        {
            get => SendToPreparationTimerSeconds.HasValue ? TimeSpan.FromSeconds(SendToPreparationTimerSeconds.Value) : null;
            set => SendToPreparationTimerSeconds = value.HasValue ? (int)value.Value.TotalSeconds : null;
        }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public int PosIntegrationId { get; set; }
        public PosIntegration? PosIntegration { get; set; }

        public ICollection<OrderConfigurableFieldChannelProfileAssociation>? AssociatedOrderConfigurableFields { get; set; }
        public ICollection<AvailabilityProfileAssociation>? AssociatedAvailabilityGroups { get; set; }
        public ICollection<Channel>? Channels { get; set; }
        #endregion
    }
}