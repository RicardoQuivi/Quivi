using Quivi.Domain.Entities.Charges;

namespace Quivi.Domain.Entities.Financing
{
    public class Journal : IEntity
    {
        public int JournalId { get; set; }

        public JournalType Type { get; set; }
        public JournalState State { get; set; }
        public JournalMethod? Method { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OrderRef { get; set; }
        public string? Description { get; set; }
        public ChargeMethod ChargeMethod { get; set; } //TODO: I Removed default value
        public string? UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }


        #region Relationships
        public int? JournalLinkId { get; set; }
        public Journal? JournalLink { get; set; }

        public JournalDetails? JournalDetails { get; set; }
        public ICollection<Posting> Postings { get; set; }
        public ICollection<JournalChange> JournalChanges { get; set; }
        public ICollection<SettlementServiceDetail> SettlementServiceDetails { get; set; }
        public ICollection<SettlementDetail> SettlementDetails { get; set; }
        public ICollection<DepositRefundJournal> DepositRefundJournals { get; set; }
        #endregion
    }
}
