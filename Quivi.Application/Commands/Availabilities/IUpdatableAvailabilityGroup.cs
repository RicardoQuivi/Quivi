namespace Quivi.Application.Commands.Availabilities
{
    public interface IUpdatableAvailabilityGroup : IUpdatableEntity
    {
        int Id { get; }
        int MerchantId { get; }
        string Name { get; set; }
        bool AutoAddNewMenuItems { get; set; }
        bool AutoAddNewChannelProfiles { get; set; }

        IUpdatableRelationship<IUpdatableMenuItemAssociation, int> MenuItems { get; }
        IUpdatableRelationship<IUpdatableChannelProfileAssociation, int> ChannelProfiles { get; }
        IUpdatableWeekdayAvailability WeekdayAvailabilities { get; }
    }

    public interface IUpdatableWeekdayAvailability : IUpdatableEntity
    {
        /// <summary>
        /// Adds a weekly availability on the provided range. If <c>from</c> is <c>TimeSpan.Zero</c>, then 
        /// it means it is available from 00:00:00 of Sunday. The maximum value <c>to</c>
        /// is 7 days (128 hours). For instance, if <c>from</c> is equal to 24:00:00 (24 hours) and<c>to</c>
        /// is equal to 104:00:00, then this represents availability from Monday 00:00:00 until Saturday 00:00:00
        /// </summary>
        /// <param name="from">The availability start range</param>
        /// <param name="to">The availability end range</param>
        void AddAvailability(TimeSpan from, TimeSpan to);

        /// <summary>
        /// Adds a weekly availability on the provided range. If <c>from</c> is <c>TimeSpan.Zero</c>, then 
        /// it means it is available from 00:00:00 of Sunday. The maximum value <c>to</c>
        /// is 7 days (128 hours). For instance, if <c>from</c> is equal to 24:00:00 (24 hours) and<c>to</c>
        /// is equal to 104:00:00, then this represents availability from Monday 00:00:00 until Saturday 00:00:00
        /// </summary>
        /// <param name="from">The availability start range</param>
        /// <param name="to">The availability end range</param>
        void RemoveAvailability(TimeSpan from, TimeSpan to);
    }

    public interface IUpdatableChannelProfileAssociation : IUpdatableEntity
    {
        public int Id { get; }
    }

    public interface IUpdatableMenuItemAssociation : IUpdatableEntity
    {
        public int Id { get; }
    }
}