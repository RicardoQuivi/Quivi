namespace Quivi.Domain.Repositories.EntityFramework.Models
{
    public class MerchantSettlementResume
    {
        public int Id { get; init; }
        public DateTime Date { get; init; }

        public int ParentMerchantId { get; init; }
        public int MerchantId { get; init; }

        public decimal ServiceAmount { get; set; }

        public decimal GrossAmount { get; init; }
        public decimal GrossTip { get; init; }
        public decimal GrossTotal => GrossAmount + GrossTip;

        public decimal NetAmount { get; init; }
        public decimal NetTip { get; init; }
        public decimal NetTotal => NetAmount + NetTip;
    }
}
