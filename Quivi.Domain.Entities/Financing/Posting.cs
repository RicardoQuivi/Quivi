namespace Quivi.Domain.Entities.Financing
{
    public class Posting : IEntity
    {
        public int Id { get; set; }

        public required string AssetType { get; set; }
        public decimal Amount { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int PersonId { get; set; }
        public Person? Person { get; set; }

        public int JournalId { get; set; }
        public Journal? Journal { get; set; }
        #endregion
    }
}
