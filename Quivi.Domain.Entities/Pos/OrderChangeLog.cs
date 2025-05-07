namespace Quivi.Domain.Entities.Pos
{
    public class OrderChangeLog : IEntity
    {
        public int Id { get; set; }

        public OrderState EatOrderState { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int OrderId { get; set; }
        public required Order Order { get; set; }
        #endregion
    }
}
