using Quivi.Domain.Entities.Pos;

namespace Quivi.Application.Commands.Availabilities
{
    public class UpdatableAvailabilityGroup : IUpdatableAvailabilityGroup
    {
        public AvailabilityGroup Model { get; }
        private readonly string originalName;
        private readonly bool originalAutoAddNewMenuItems;
        private readonly bool originalAutoAddNewChannelProfiles;

        public readonly UpdatableRelationshipEntity<AvailabilityProfileAssociation, IUpdatableChannelProfileAssociation, int> UpdatableChannelProfileAssociations;
        public readonly UpdatableRelationshipEntity<AvailabilityMenuItemAssociation, IUpdatableMenuItemAssociation, int> UpdatableMenuItemsAssociations;
        private UpdatableWeekdayAvailabilities updatableWeekdayAvailability;

        public UpdatableAvailabilityGroup(AvailabilityGroup model, DateTime now)
        {
            Model = model;
            updatableWeekdayAvailability = new UpdatableWeekdayAvailabilities(model);
            originalName = model.Name;
            originalAutoAddNewMenuItems = model.AutoAddNewMenuItems;
            originalAutoAddNewChannelProfiles = model.AutoAddNewChannelProfiles;

            this.UpdatableChannelProfileAssociations = new UpdatableRelationshipEntity<AvailabilityProfileAssociation, IUpdatableChannelProfileAssociation, int>(model.AssociatedChannelProfiles!, m => m.ChannelProfileId, t => new UpdatableChannelProfileAssociation(t), (id) => new AvailabilityProfileAssociation
            {
                ChannelProfileId = id,

                AvailabilityGroup = this.Model,
                AvailabilityGroupId = this.Model.Id,

                CreatedDate = now,
                ModifiedDate = now,
            });

            this.UpdatableMenuItemsAssociations = new UpdatableRelationshipEntity<AvailabilityMenuItemAssociation, IUpdatableMenuItemAssociation, int>(model.AssociatedMenuItems!, m => m.MenuItemId, t => new UpdatableMenuItemAssociation(t), (id) => new AvailabilityMenuItemAssociation
            {
                MenuItemId = id,

                AvailabilityGroup = this.Model,
                AvailabilityGroupId = this.Model.Id,

                CreatedDate = now,
                ModifiedDate = now,
            });
        }

        public int Id => this.Model.Id;
        public int MerchantId => this.Model.MerchantId;
        public string Name
        {
            get => Model.Name;
            set => Model.Name = value;
        }
        public bool AutoAddNewMenuItems
        {
            get => Model.AutoAddNewMenuItems;
            set => Model.AutoAddNewMenuItems = value;
        }
        public bool AutoAddNewChannelProfiles
        {
            get => Model.AutoAddNewChannelProfiles;
            set => Model.AutoAddNewChannelProfiles = value;
        }

        public bool NameChanged => originalName != Model.Name;
        public bool HasChanges
        {
            get
            {
                if (NameChanged)
                    return true;

                if (AutoAddNewMenuItems != originalAutoAddNewMenuItems)
                    return true;

                if (AutoAddNewChannelProfiles != originalAutoAddNewChannelProfiles)
                    return true;

                if (this.UpdatableChannelProfileAssociations.HasChanges)
                    return true;

                if (this.UpdatableMenuItemsAssociations.HasChanges)
                    return true;

                if (this.updatableWeekdayAvailability.HasChanges)
                    return true;

                return false;
            }
        }

        public IUpdatableWeekdayAvailability WeekdayAvailabilities => updatableWeekdayAvailability;
        public IUpdatableRelationship<IUpdatableChannelProfileAssociation, int> ChannelProfiles => this.UpdatableChannelProfileAssociations;
        public IUpdatableRelationship<IUpdatableMenuItemAssociation, int> MenuItems => this.UpdatableMenuItemsAssociations;


        private class UpdatableWeekdayAvailabilities : IUpdatableWeekdayAvailability
        {
            private readonly AvailabilityGroup Model;
            private readonly Dictionary<WeeklyAvailability, UpdatableWeeklyAvailability> availabilityIntervals;

            public UpdatableWeekdayAvailabilities(AvailabilityGroup group)
            {
                this.Model = group;
                this.availabilityIntervals = group.WeeklyAvailabilities?.ToDictionary(a => a, a => new UpdatableWeeklyAvailability(a, false)) ?? throw new Exception($"{nameof(AvailabilityGroup.WeeklyAvailabilities)} were not included or initialized.");
            }

            public void AddAvailability(TimeSpan from, TimeSpan to)
            {
                var sortedAvailabilities = availabilityIntervals.Values.OrderBy(a => a.StartAt.Ticks).ToList();
                UpdatableWeeklyAvailability? mergingAvailability = null;

                foreach (var availability in sortedAvailabilities)
                {
                    var availStartTicks = availability.StartAt.Ticks;
                    var availEndTicks = availability.EndAt.Ticks;

                    if (mergingAvailability == null)
                    {
                        // Skip if no overlap or adjacency
                        if (availEndTicks <= from.Ticks || availStartTicks >= to.Ticks)
                            continue;

                        //If they are the same exact interval, nothing to add
                        if (availStartTicks == from.Ticks && to.Ticks == availEndTicks)
                            return;

                        //If the input interval is fully contained within an existing one, nothing to add
                        if (availStartTicks <= from.Ticks && to.Ticks <= availEndTicks)
                            return;

                        // Start merging
                        mergingAvailability = availability;
                        continue;
                    }

                    // Continue merging chain
                    if (availStartTicks <= to.Ticks)
                    {
                        mergingAvailability.EndAt = availability.EndAt > to ? availability.EndAt : to;

                        this.Model.WeeklyAvailabilities!.Remove(availability.Model);
                        this.availabilityIntervals.Remove(availability.Model);
                        availability.MarkAsDeleted();

                        continue;
                    }

                    // No further overlaps — stop
                    break;
                }

                if (mergingAvailability == null)
                {
                    var weeklyAvailability = new WeeklyAvailability
                    {
                        AvailabilityGroup = this.Model,
                        AvailabilityGroupId = this.Model.Id,

                        StartAt = from,
                        EndAt = to,
                    };
                    this.Model.WeeklyAvailabilities!.Add(weeklyAvailability);
                    this.availabilityIntervals.Add(weeklyAvailability, new UpdatableWeeklyAvailability(weeklyAvailability, true));
                }
                else
                    mergingAvailability.EndAt = TimeSpan.FromTicks(Math.Max(mergingAvailability.EndAt.Ticks, to.Ticks));
            }

            public void RemoveAvailability(TimeSpan from, TimeSpan to)
            {
                var sortedAvailabilities = availabilityIntervals.Values.OrderBy(a => a.StartAt.Ticks).ToList();
                bool goingThroughRemoval = false;

                foreach (var availability in sortedAvailabilities)
                {
                    var availStartTicks = availability.StartAt.Ticks;
                    var availEndTicks = availability.EndAt.Ticks;

                    if (goingThroughRemoval == false)
                    {
                        //If the removing interval does not start with this interval, then continue searching
                        if ((availStartTicks <= from.Ticks && from.Ticks <= availEndTicks) == false)
                            continue;

                        //If it's exactly the same interval, then just remove it
                        if (availStartTicks == from.Ticks && to.Ticks == availEndTicks)
                        {
                            this.Model.WeeklyAvailabilities!.Remove(availability.Model);
                            availability.MarkAsDeleted();
                            return;
                        }

                        //If the adding interval fits is a subinterval of the current availability, then we need to trim it or split it
                        if (availStartTicks <= from.Ticks && to.Ticks <= availEndTicks)
                        {
                            //If the interval either starts or ends with the availability, then just trim it
                            if (availStartTicks == from.Ticks || to.Ticks == availEndTicks)
                            {
                                if (availStartTicks == from.Ticks)
                                    availability.StartAt = to;
                                else
                                    availability.EndAt = from;

                                return;
                            }

                            //Else split it
                            var weeklyAvailability = new WeeklyAvailability
                            {
                                AvailabilityGroup = this.Model,
                                AvailabilityGroupId = this.Model.Id,

                                StartAt = to,
                                EndAt = availability.EndAt,
                            };
                            this.Model.WeeklyAvailabilities!.Add(weeklyAvailability);
                            this.availabilityIntervals.Add(weeklyAvailability, new UpdatableWeeklyAvailability(weeklyAvailability, true));

                            availability.EndAt = from;
                            continue;
                        }

                        //Else this interval needs to shrink
                        availability.EndAt = from;
                        goingThroughRemoval = true;
                        continue;
                    }

                    //If the removing interval goes through the end of this interval, then this interval is completely removed
                    if (availEndTicks <= to.Ticks)
                    {
                        this.Model.WeeklyAvailabilities!.Remove(availability.Model);
                        availability.MarkAsDeleted();
                        continue;
                    }

                    availability.StartAt = to;
                    goingThroughRemoval = false;
                }
            }

            public bool HasChanges
            {
                get
                {
                    if (availabilityIntervals.Count != Model.WeeklyAvailabilities?.Count)
                        return true;

                    return availabilityIntervals.Values.Any(a => a.HasChanges);
                }
            }

            private class UpdatableWeeklyAvailability : IUpdatableEntity
            {
                public readonly WeeklyAvailability Model;
                private bool isNew;
                private bool isDeleted;
                private readonly TimeSpan originalStartAt;
                private readonly TimeSpan originalEndAt;

                public UpdatableWeeklyAvailability(WeeklyAvailability model, bool isNew)
                {
                    this.Model = model;
                    this.isNew = isNew;
                    this.originalStartAt = model.StartAt;
                    this.originalEndAt = model.EndAt;
                    this.isDeleted = false;
                }
                public void MarkAsDeleted() => this.isDeleted = true;

                public TimeSpan StartAt
                {
                    get => Model.StartAt;
                    set => Model.StartAt = value;
                }

                public TimeSpan EndAt
                {
                    get => Model.EndAt;
                    set => Model.EndAt = value;
                }

                public bool HasChanges
                {
                    get
                    {
                        if (isNew)
                        {
                            if (isDeleted == false)
                                return true;
                        }
                        else if (isDeleted)
                            return true;

                        if (originalEndAt != Model.EndAt)
                            return true;

                        if (originalStartAt != Model.StartAt)
                            return true;

                        return false;
                    }
                }
            }
        }

        private class UpdatableChannelProfileAssociation : IUpdatableChannelProfileAssociation
        {
            public readonly AvailabilityProfileAssociation Model;
            private readonly bool isNew;

            public UpdatableChannelProfileAssociation(AvailabilityProfileAssociation model)
            {
                this.Model = model;
                isNew = model.AvailabilityGroupId == 0;
            }

            public int Id => Model.ChannelProfileId;

            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    return false;
                }
            }
        }

        private class UpdatableMenuItemAssociation : IUpdatableMenuItemAssociation
        {
            public readonly AvailabilityMenuItemAssociation Model;
            private readonly bool isNew;

            public UpdatableMenuItemAssociation(AvailabilityMenuItemAssociation model)
            {
                this.Model = model;
                isNew = model.AvailabilityGroupId == 0;
            }

            public int Id => Model.MenuItemId;

            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    return false;
                }
            }
        }
    }
}