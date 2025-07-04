﻿using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.ChannelProfiles;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Events.Data.Channels;

namespace Quivi.Application.Commands.ChannelProfiles
{
    public interface IUpdatableChannelProfile : IUpdatableEntity
    {
        ChannelFeature Features { get; set; }
        decimal MinimumPrePaidOrderAmount { get; set; }
        TimeSpan? SendToPreparationTimer { get; set; }
        string Name { get; set; }
        int PosIntegrationId { get; set; }
    }

    public class UpdateChannelProfileAsyncCommand : AUpdateAsyncCommand<IEnumerable<ChannelProfile>, IUpdatableChannelProfile>
    {
        public required GetChannelProfilesCriteria Criteria { get; init; }
    }

    public class UpdateChannelProfileAsyncCommandHandler : ICommandHandler<UpdateChannelProfileAsyncCommand, Task<IEnumerable<ChannelProfile>>>
    {
        private readonly IChannelProfilesRepository repository;
        private readonly IChannelsRepository channelsRepository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateChannelProfileAsyncCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = unitOfWork.ChannelProfiles;
            this.channelsRepository = unitOfWork.Channels;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        private class UpdatableChannelProfile : IUpdatableChannelProfile
        {
            public ChannelProfile Model { get; }

            private readonly ChannelFeature originalFeatures;
            private readonly decimal originalMinimumPrePaidOrderAmount;
            private readonly TimeSpan? originalSendToPreparationTimer;
            private readonly string originalName;
            private readonly int? originalPosIntegrationId;

            public UpdatableChannelProfile(ChannelProfile model)
            {
                this.Model = model;
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

            public bool HasChanges => MinimumPrePaidOrderAmount != originalMinimumPrePaidOrderAmount ||
                                        originalSendToPreparationTimer != SendToPreparationTimer ||
                                        originalFeatures != Features ||
                                        originalName != Name ||
                                        originalPosIntegrationId != PosIntegrationId;
        }

        public async Task<IEnumerable<ChannelProfile>> Handle(UpdateChannelProfileAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria);
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var changedEntities = new List<UpdatableChannelProfile>();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableChannelProfile(entity);
                await command.UpdateAction.Invoke(updatableEntity);

                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();
            await GenerateQrCodeChangedEvents(changedEntities);

            foreach (var entity in changedEntities)
                await eventService.Publish(new OnChannelProfileOperationEvent
                {
                    Id = entity.Model.Id,
                    MerchantId = entity.Model.MerchantId,
                    Operation = EntityOperation.Update,
                });

            return entities;
        }

        private async Task GenerateQrCodeChangedEvents(IEnumerable<UpdatableChannelProfile> changedEntities)
        {
            //TODO: This logic most likely doesn't make sense. This event shoudl probably not be triggered or be triggered elsewhere
            var channels = await channelsRepository.GetAsync(new GetChannelsCriteria
            {
                ChannelProfileIds = changedEntities.Select(p => p.Model.Id),
            });

            foreach (var entity in channels)
                await eventService.Publish(new OnChannelOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Update,
                });
        }
    };
}
