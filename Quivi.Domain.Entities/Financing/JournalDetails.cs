namespace Quivi.Domain.Entities.Financing
{
    public class JournalDetails : IEntity
    {
        public decimal IncludedTip { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int JournalId { get; set; }
        public Journal? Journal { get; set; }
        #endregion
    }
}