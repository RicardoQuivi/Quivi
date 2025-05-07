namespace Quivi.Domain.Entities.Pos
{
    public class OrderSequence
    {
        public const int SequenceNumberDigits = 4;
        public static readonly int SequenceNumberDivider = (int)Math.Pow(10, SequenceNumberDigits);


        public int Id => OrderId;
        public long SequenceNumber { get; set; }
        public string PaddedSequenceNumber => $"{SequenceNumber % SequenceNumberDivider}".PadLeft(SequenceNumberDigits, '0');

        #region Relationships
        public int OrderId { get; set; }
        public required Order Order { get; set; }
        #endregion
    }
}
