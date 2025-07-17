namespace Quivi.Domain.Entities.Pos
{
    public class OrderSequence : IEntity
    {
        public const int SequenceNumberDigits = 4;
        public static readonly int SequenceNumberDivider = (int)Math.Pow(10, SequenceNumberDigits);


        public int Id => OrderId;
        public long SequenceNumber { get; set; }
        public string PaddedSequenceNumber => $"{SequenceNumber % SequenceNumberDivider}".PadLeft(SequenceNumberDigits, '0');

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        #endregion
    }
}
