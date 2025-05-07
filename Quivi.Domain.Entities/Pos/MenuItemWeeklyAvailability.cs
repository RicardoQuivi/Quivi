namespace Quivi.Domain.Entities.Pos
{
    public class MenuItemWeeklyAvailability : IWeeklyAvailability, IDomainWeeklyAvailability, IDeletableEntity
    {
        public int Id { get; set; }

        public int StartAtSeconds { get; set; }
        public int EndAtSeconds { get; set; }

        public TimeSpan StartAt
        {
            get => TimeSpan.FromSeconds(StartAtSeconds);
            set => StartAtSeconds = (int)value.TotalSeconds;
        }
        public TimeSpan EndAt
        {
            get => TimeSpan.FromSeconds(EndAtSeconds);
            set => EndAtSeconds = (int)value.TotalSeconds;
        }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        public int MenuItemId { get; set; }
        public required MenuItem MenuItem { get; set; }
    }
}