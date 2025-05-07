using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class Order : IEntity
    {
        public int Id { get; set; }

        public OrderType OrderType { get; set; }
        public OrderState State { get; set; }

        public bool PayLater { get; set; }
        public OrderOrigin Origin { get; set; }

        public DateTime? ScheduledTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }


        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public int ChannelId { get; set; }
        public Channel? Channel { get; set; }

        public int? SessionId { get; set; }
        public Session? Session { get; set; }

        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public OrderSequence? OrderSequence { get; set; }

        public ICollection<PreparationGroup>? PreparationGroups { get; set; }
        public ICollection<OrderMenuItem>? OrderMenuItems { get; set; }
        public ICollection<OrderChangeLog>? OrderChangeLogs { get; set; }
        public ICollection<OrderAdditionalInfo>? OrderAdditionalInfos { get; set; }
        #endregion
    }
}
