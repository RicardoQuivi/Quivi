using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Channels;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.Channels
{
    public class AddChannelsAsyncCommand : ICommand<Task<IEnumerable<Channel>>>
    {
        public int MerchantId { get; init; }
        public int ChannelProfileId { get; init; }
        public required IEnumerable<ChannelToAdd> Data { get; init; }
    }

    public class ChannelToAdd
    {
        public required string Identifier { get; init; }
    }

    public class AddChannelsAsyncCommandHandler : ICommandHandler<AddChannelsAsyncCommand, Task<IEnumerable<Channel>>>
    {
        private readonly IChannelsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddChannelsAsyncCommandHandler(IChannelsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<Channel>> Handle(AddChannelsAsyncCommand command)
        {
            var utcNow = dateTimeProvider.GetUtcNow();

            var existingChannelsQuery = await repository.GetAsync(new Infrastructure.Abstractions.Repositories.Criterias.GetChannelsCriteria
            {
                MerchantIds = [ command.MerchantId ],
                ChannelProfileIds = [ command.ChannelProfileId ],
                Identifiers = command.Data.Select(s => s.Identifier),
                IsDeleted = null,
                PageSize = null,
            });
            var channelsMap = existingChannelsQuery.ToDictionary(s => s.Identifier, s => s);

            List<Channel> addedChannels = new List<Channel>();
            foreach(var channelToAdd in command.Data)
            {
                if (channelsMap.TryGetValue(channelToAdd.Identifier, out var channel) == false)
                {
                    channel = new Channel
                    {
                        Identifier = channelToAdd.Identifier,
                        PoSIdentifier = null,
                        ChannelProfileId = command.ChannelProfileId,
                        MerchantId = command.MerchantId,
                        CreatedDate = utcNow,
                        ModifiedDate = utcNow,
                    };
                    repository.Add(channel);
                }
                channel.DeletedDate = null;
                addedChannels.Add(channel);
            }

            await repository.SaveChangesAsync();

            foreach (var entity in addedChannels)
                await eventService.Publish(new OnChannelOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = command.MerchantId,
                    Operation = EntityOperation.Create,
                });

            return addedChannels;
        }
    }
}
