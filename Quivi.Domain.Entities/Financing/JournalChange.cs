namespace Quivi.Domain.Entities.Financing
{
    public class JournalChange : IEntity
    {
        public int Id { get; set; }

        public JournalType Type { get; set; }
        public JournalState State { get; set; }
        public decimal Amount { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationship
        public int JournalId { get; set; }
        public Journal? Journal { get; set; }

        public int? JournalLinkId { get; set; }
        public Journal? JournalLink { get; set; }
        #endregion
    }
}