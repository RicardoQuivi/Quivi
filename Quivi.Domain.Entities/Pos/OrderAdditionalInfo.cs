namespace Quivi.Domain.Entities.Pos
{
    public class OrderAdditionalInfo
    {
        public required string Value { get; set; }

        #region Relationships
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int OrderConfigurableFieldId { get; set; }
        public OrderConfigurableField? OrderConfigurableField { get; set; }
        #endregion
    }
}
