using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.ChannelProfiles;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.ChannelProfiles
{
    public class AddChannelProfileAsyncCommand : ICommand<Task<ChannelProfile>>
    {
        public int MerchantId { get; init; }
        public required string Name { get; init; }
        public decimal MinimumPrePaidOrderAmount { get; init; }
        public ChannelFeature Features { get; init; }
        public TimeSpan? SendToPreparationTimer { get; init; }
        public int PosIntegrationId { get; init; }
    }

    public class AddChannelProfileAsyncCommandHandler : ICommandHandler<AddChannelProfileAsyncCommand, Task<ChannelProfile>>
    {
        private readonly IChannelProfilesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddChannelProfileAsyncCommandHandler(IChannelProfilesRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<ChannelProfile> Handle(AddChannelProfileAsyncCommand command)
        {
            var now = dateTimeProvider.GetUtcNow();
            var entity = new ChannelProfile
            {
                MerchantId = command.MerchantId,
                Name = command.Name,
                Features = command.Features,
                PrePaidOrderingMinimumAmount = command.MinimumPrePaidOrderAmount,
                SendToPreparationTimer = command.SendToPreparationTimer,
                PosIntegrationId = command.PosIntegrationId,
                CreatedDate = now,
                ModifiedDate = now, 
                DeletedDate = null,
            };
            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnChannelProfileOperationEvent
            {
                Id = entity.Id,
                MerchantId = entity.MerchantId,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    };
}
