using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Channels;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Channels
{
    public interface IUpdatableChannel : IUpdatableEntity
    {
        int MerchantId { get; }
        int Id { get; }
        int ChannelProfileId { get; set; }
        bool IsDeleted { get; set; }
        string Identifier { get; set; }
    }

    public class UpdateChannelAsyncCommand : AUpdateAsyncCommand<IEnumerable<Channel>, IUpdatableChannel>
    {
        public required GetChannelsCriteria Criteria { get; init; }
    }

    public class UpdateChannelAsyncCommandHandler : ICommandHandler<UpdateChannelAsyncCommand, Task<IEnumerable<Channel>>>
    {
        private readonly IChannelsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        private class UpdatableChannel : IUpdatableChannel
        {
            public readonly Channel Model;
            private readonly bool originalDeleted;
            private readonly string originalIdentifier;
            private readonly int originalProfileId;
            private readonly DateTime now;

            public UpdatableChannel(Channel original, DateTime now)
            {
                Model = original;
                originalDeleted = original.DeletedDate.HasValue;
                originalIdentifier = original.Identifier;
                originalProfileId = original.ChannelProfileId;
                this.now = now;
            }

            public int MerchantId => Model.MerchantId;
            public int Id => Model.Id;

            public int ChannelProfileId
            {
                get => Model.ChannelProfileId;
                set => Model.ChannelProfileId = value;
            }

            public bool IsDeleted
            {
                get => Model.DeletedDate.HasValue;
                set => Model.DeletedDate = value ? now : null;
            }

            public string Identifier
            {
                get => Model.Identifier;
                set => Model.Identifier = value;
            }

            public bool ProfileChanged => originalProfileId != Model.ChannelProfileId;
            public bool WasDeleted => originalDeleted == false && IsDeleted;
            public bool HasChanges => originalIdentifier != Model.Identifier || originalDeleted != IsDeleted || ProfileChanged;
        }

        public UpdateChannelAsyncCommandHandler(IChannelsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<Channel>> Handle(UpdateChannelAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria);
            if (entities.Any() == false)
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var changedEntities = new List<UpdatableChannel>();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableChannel(entity, now);
                await command.UpdateAction.Invoke(updatableEntity);

                if (updatableEntity.HasChanges)
                {
                    changedEntities.Add(updatableEntity);

                    if (updatableEntity.ProfileChanged)
                        updatableEntity.Model.PoSIdentifier = null;
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();

            foreach (var entity in changedEntities)
                await eventService.Publish(new OnChannelOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = entity.WasDeleted ? EntityOperation.Delete : EntityOperation.Update,
                });
            return entities;
        }
    }
}
