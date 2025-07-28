using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Orders
{
    [Obsolete($"Use {nameof(AddOrdersAsyncCommand)} instead.")]
    public class AddOrderAsyncCommand : ICommand<Task<Order>>
    {
        public int ChannelId { get; init; }
    }

    public class AddOrderAsyncCommandHandler : ICommandHandler<AddOrderAsyncCommand, Task<Order>>
    {
        private readonly IOrdersRepository repository;
        private readonly IChannelProfilesRepository channelProfileRepository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddOrderAsyncCommandHandler(IOrdersRepository repository,
                                            IChannelProfilesRepository channelProfileRepository,
                                            IDateTimeProvider dateTimeProvider,
                                            IEventService eventService)
        {
            this.repository = repository;
            this.channelProfileRepository = channelProfileRepository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<Order> Handle(AddOrderAsyncCommand command)
        {
            var profileQuery = await channelProfileRepository.GetAsync(new GetChannelProfilesCriteria
            {
                ChannelIds = [command.ChannelId],
            });

            var channelProfile = profileQuery.SingleOrDefault();
            if (channelProfile == null)
                throw new Exception("Invalid Channel");

            bool isTakeAway = channelProfile.Features.HasFlag(ChannelFeature.IsTakeAwayOnly) == true;
            var now = dateTimeProvider.GetUtcNow();
            var order = new Order
            {
                MerchantId = channelProfile.MerchantId,
                ChannelId = command.ChannelId,
                OrderType = isTakeAway ? OrderType.TakeAway : OrderType.OnSite,
                State = OrderState.Draft,
                Origin = OrderOrigin.GuestsApp,
                EmployeeId = null,
                CreatedDate = now,
                ModifiedDate = now,
            };
            repository.Add(order);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnOrderOperationEvent
            {
                MerchantId = order.MerchantId,
                ChannelId = command.ChannelId,
                Id = order.Id,
                Operation = EntityOperation.Create,
            });
            return order;
        }
    }
}