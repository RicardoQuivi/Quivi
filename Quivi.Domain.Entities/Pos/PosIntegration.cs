using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class PosIntegration : IDeletableEntity
    {
        public int Id { get; set; }

        public IntegrationType IntegrationType { get; set; }
        public required string ConnectionString { get; set; }
        public SyncState SyncState { get; set; }
        public DateTime? LastSyncingDate { get; set; }
        public DateTime? SyncStateModifiedDate { get; set; }
        public bool DiagnosticErrorsMuted { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public ICollection<ChannelProfile>? ChannelProfiles { get; set; }
        #endregion
    }
}
