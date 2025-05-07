using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.ChannelProfiles;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.ChannelProfiles
{
    public class DeleteChannelProfileAsyncCommand : ICommand<Task<IEnumerable<ChannelProfile>>>
    {
        public required GetChannelProfilesCriteria Criteria { get; init; }
        public required Action<int> OnChannelsAssociatedError { get; init; }
    }

    public class DeleteChannelProfileAsyncCommandHandler : ICommandHandler<DeleteChannelProfileAsyncCommand, Task<IEnumerable<ChannelProfile>>>
    {
        private readonly IChannelProfilesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public DeleteChannelProfileAsyncCommandHandler(IChannelProfilesRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<ChannelProfile>> Handle(DeleteChannelProfileAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IsDeleted = false,
                IncludeChannels = true,
            });
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var entitiesWithChannels = new List<ChannelProfile>();
            foreach(var e in entities)
            {
                if(e.Channels!.Any())
                {
                    entitiesWithChannels.Add(e);
                    break;
                }
                e.DeletedDate = now;
            }

            if(entitiesWithChannels.Any())
            {
                foreach(var e in entitiesWithChannels)
                    command.OnChannelsAssociatedError(e.Id);
                return [];
            }

            await repository.SaveChangesAsync();

            foreach (var entity in entities)
                await eventService.Publish(new OnChannelProfileOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Delete,
                });

            return entities;
        }
    };
}
