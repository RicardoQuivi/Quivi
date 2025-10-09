using Quivi.Domain.Entities.Pos;

namespace Quivi.Application.Commands.ChannelProfiles
{
    public interface IUpdatableOrderConfigurableFieldAssociation : IUpdatableEntity
    {
        public int Id { get; }
    }

    public interface IUpdatableAvailabilityGroupAssociation : IUpdatableEntity
    {
        public int Id { get; }
    }


    public interface IUpdatableChannelProfile : IUpdatableEntity
    {
        ChannelFeature Features { get; set; }
        decimal MinimumPrePaidOrderAmount { get; set; }
        TimeSpan? SendToPreparationTimer { get; set; }
        string Name { get; set; }
        int PosIntegrationId { get; set; }
        IUpdatableRelationship<IUpdatableOrderConfigurableFieldAssociation, int> OrderConfigurableFields { get; }
        IUpdatableRelationship<IUpdatableAvailabilityGroupAssociation, int> AvailabilityGroups { get; }
    }
}