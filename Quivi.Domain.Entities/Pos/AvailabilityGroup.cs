using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Pos
{
    public class AvailabilityGroup : IEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public bool AutoAddNewMenuItems { get; set; }
        public bool AutoAddNewChannelProfiles { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public ICollection<WeeklyAvailability>? WeeklyAvailabilities { get; set; }

        public ICollection<AvailabilityProfileAssociation>? AssociatedChannelProfiles { get; set; }
        public ICollection<AvailabilityMenuItemAssociation>? AssociatedMenuItems { get; set; }
        #endregion
    }

    /// <summary>
    /// Represents a weekly availability. If <c>StartAt</c> is <c>TimeSpan.Zero</c>, then 
    /// it means it is available from 00:00:00 of Sunday. The maximum value <c>EndAt</c>
    /// is 7 days (128 hours). For instance, if <c>StartAt</c> is equal to 24:00:00 (24 hours) and<c>EndAt</c>
    /// is equal to 104:00:00, then this represents availability from Monday 00:00:00 until Saturday 00:00:00
    /// </summary>
    public class WeeklyAvailability : IWeeklyAvailability, IDomainWeeklyAvailability, IBaseEntity
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

        #region Relationships
        public int AvailabilityGroupId { get; set; }
        public AvailabilityGroup? AvailabilityGroup { get; set; }
        #endregion
    }
}