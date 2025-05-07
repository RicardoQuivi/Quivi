namespace Quivi.Domain.Entities.Pos
{
    public enum SyncAttemptState
    {
        Failed = 0,
        Syncing = 0,
        Synced = 1,
    }

    public class PosChargeSyncAttempt : IEntity
    {
        public int Id { get; set; }
        public SyncAttemptState State { get; set; }
        public decimal SyncedAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int PosChargeId { get; set; }
        public PosCharge? PosCharge { get; set; }
        #endregion
    }
}