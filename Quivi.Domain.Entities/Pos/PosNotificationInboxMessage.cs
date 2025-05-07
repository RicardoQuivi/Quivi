namespace Quivi.Domain.Entities.Pos
{
    public class PosNotificationInboxMessage : IEntity
    {
        public DateTime? ReadAt { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int EmployeeId { get; set; }
        public required Employee Employee { get; set; }

        public int PosNotificationMessageId { get; set; }
        public required PosNotificationMessage PosNotificationMessage { get; set; }
        #endregion
    }
}
