namespace Quivi.Domain.Entities.Pos
{
    public class Session : IEntity
    {
        public int Id { get; set; }

        public string? PosIdentifier { get; set; }
        public SessionStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int ChannelId { get; set; }
        public Channel? Channel { get; set; }

        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public ICollection<PreparationGroup>? PreparationGroups { get; set; }
        public ICollection<Order>? Orders { get; set; }
        #endregion
    }
}