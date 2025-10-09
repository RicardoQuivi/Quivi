using Quivi.Domain.Entities.Pos;

namespace Quivi.Application.Commands.ChannelProfiles
{
    public class UpdatableChannelProfile : IUpdatableChannelProfile
    {
        private class UpdatableOrderConfigurableFieldAssociation : IUpdatableOrderConfigurableFieldAssociation
        {
            public readonly OrderConfigurableFieldChannelProfileAssociation Model;
            private readonly bool isNew;

            public UpdatableOrderConfigurableFieldAssociation(OrderConfigurableFieldChannelProfileAssociation model)
            {
                this.Model = model;
                isNew = model.ChannelProfileId == 0;
            }

            public int Id => Model.OrderConfigurableFieldId;

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

        private class UpdatableAvailabilityGroupAssociation : IUpdatableAvailabilityGroupAssociation
        {
            public readonly AvailabilityProfileAssociation Model;
            private readonly bool isNew;

            public UpdatableAvailabilityGroupAssociation(AvailabilityProfileAssociation model)
            {
                this.Model = model;
                isNew = model.ChannelProfileId == 0;
            }

            public int Id => Model.AvailabilityGroupId;

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

        public ChannelProfile Model { get; }
        public readonly UpdatableRelationshipEntity<OrderConfigurableFieldChannelProfileAssociation, IUpdatableOrderConfigurableFieldAssociation, int> UpdatableOrderConfigurableFields;
        public readonly UpdatableRelationshipEntity<AvailabilityProfileAssociation, IUpdatableAvailabilityGroupAssociation, int> UpdatableAvailabilityGroups;

        private readonly ChannelFeature originalFeatures;
        private readonly decimal originalMinimumPrePaidOrderAmount;
        private readonly TimeSpan? originalSendToPreparationTimer;
        private readonly string originalName;
        private readonly int? originalPosIntegrationId;

        public UpdatableChannelProfile(ChannelProfile model, DateTime now)
        {
            this.Model = model;
            this.UpdatableOrderConfigurableFields = new UpdatableRelationshipEntity<OrderConfigurableFieldChannelProfileAssociation, IUpdatableOrderConfigurableFieldAssociation, int>(model.AssociatedOrderConfigurableFields!, m => m.OrderConfigurableFieldId, t => new UpdatableOrderConfigurableFieldAssociation(t), (id) => new OrderConfigurableFieldChannelProfileAssociation
            {
                OrderConfigurableFieldId = id,
                ChannelProfile = this.Model,
                ChannelProfileId = this.Model.Id,

                CreatedDate = now,
                ModifiedDate = now,
            });

            this.UpdatableAvailabilityGroups = new UpdatableRelationshipEntity<AvailabilityProfileAssociation, IUpdatableAvailabilityGroupAssociation, int>(model.AssociatedAvailabilityGroups!, m => m.AvailabilityGroupId, t => new UpdatableAvailabilityGroupAssociation(t), (id) => new AvailabilityProfileAssociation
            {
                AvailabilityGroupId = id,
                ChannelProfile = this.Model,
                ChannelProfileId = this.Model.Id,

                CreatedDate = now,
                ModifiedDate = now,
            });

            this.originalFeatures = model.Features;
            this.originalMinimumPrePaidOrderAmount = model.PrePaidOrderingMinimumAmount ?? 0.0m;
            this.originalSendToPreparationTimer = model.SendToPreparationTimer;
            this.originalName = model.Name;
            this.originalPosIntegrationId = model.PosIntegrationId;
        }

        public ChannelFeature Features
        {
            get => Model.Features;
            set => Model.Features = value;
        }
        public decimal MinimumPrePaidOrderAmount
        {
            get => Model.PrePaidOrderingMinimumAmount ?? 0.0M;
            set => Model.PrePaidOrderingMinimumAmount = value;
        }
        public TimeSpan? SendToPreparationTimer
        {
            get => Model.SendToPreparationTimer;
            set => Model.SendToPreparationTimer = value;
        }
        public string Name
        {
            get => Model.Name;
            set => Model.Name = value;
        }
        public int PosIntegrationId
        {
            get => Model.PosIntegrationId;
            set => Model.PosIntegrationId = value;
        }

        public IUpdatableRelationship<IUpdatableOrderConfigurableFieldAssociation, int> OrderConfigurableFields => UpdatableOrderConfigurableFields;

        public IUpdatableRelationship<IUpdatableAvailabilityGroupAssociation, int> AvailabilityGroups => UpdatableAvailabilityGroups;

        public bool HasChanges
        {
            get
            {
                if (originalMinimumPrePaidOrderAmount != MinimumPrePaidOrderAmount)
                    return true;

                if (originalSendToPreparationTimer != SendToPreparationTimer)
                    return true;

                if (originalFeatures != Features)
                    return true;

                if (originalName != Name)
                    return true;

                if (originalPosIntegrationId != PosIntegrationId)
                    return true;

                if (UpdatableOrderConfigurableFields.HasChanges)
                    return true;

                if (UpdatableAvailabilityGroups.HasChanges)
                    return true;

                return false;
            }
        }
    }
}