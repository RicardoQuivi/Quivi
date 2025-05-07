namespace Quivi.Domain.Entities.Pos
{
    public enum FieldType
    {
        String = 0,
        BigString = 1,
        Boolean = 2,
        NumbersOnly = 3,
    }

    [Flags]
    public enum AssignedOn
    {
        None = 0,
        PoSSessions = 1 << 1,
        Ordering = 1 << 2,
    }

    [Flags]
    public enum PrintedOn
    {
        None = 0,
        PreparationRequest = 1 << 1,
        TableBill = 1 << 2,
    }

    public class OrderConfigurableField : IDeletableEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public bool IsRequired { get; set; }
        public bool IsAutoFill { get; set; }
        public AssignedOn AssignedOn { get; set; }
        public PrintedOn PrintedOn { get; set; }
        public FieldType Type { get; set; }
        public string? DefaultValue { get; set; }


        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int ChannelProfileId { get; set; }
        public required ChannelProfile ChannelProfile { get; set; }

        public ICollection<OrderConfigurableFieldTranslation> Translations { get; set; }
        public ICollection<OrderAdditionalInfo> OrderAdditionalInfos { get; set; }
        #endregion
    }
}
