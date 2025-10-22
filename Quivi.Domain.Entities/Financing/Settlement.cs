namespace Quivi.Domain.Entities.Financing
{
    public class Settlement : IEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public SettlementState State { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<SettlementDetail>? SettlementDetails { get; set; }
        public ICollection<SettlementServiceDetail>? SettlementServiceDetails { get; set; }
    }
}