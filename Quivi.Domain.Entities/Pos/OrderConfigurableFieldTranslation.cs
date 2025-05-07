namespace Quivi.Domain.Entities.Pos
{
    public class OrderConfigurableFieldTranslation : IEntity, ITranslation
    {
        public Language Language { get; set; }
        public required string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int OrderConfigurableFieldId { get; set; }
        public required OrderConfigurableField OrderConfigurableField { get; set; }
        #endregion
    }
}
