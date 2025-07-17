using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.Infrastructure.Abstractions.Events.Data.PreparationGroups;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Hangfire.EventHandlers.PreparationGroups
{
    public class OnPreparationGroupCompletedEventHandler : BackgroundEventHandler<OnPreparationGroupCompletedEvent>
    {
        private readonly IEventService eventService;
        private readonly ISessionsRepository repository;

        public OnPreparationGroupCompletedEventHandler(IBackgroundJobHandler backgroundJobHandler,
                                                        ISessionsRepository repository,
                                                        IEventService eventService) : base(backgroundJobHandler)
        {
            this.repository = repository;
            this.eventService = eventService;
        }

        public override async Task Run(OnPreparationGroupCompletedEvent message)
        {
            var sessionQuery = await repository.GetAsync(new GetSessionsCriteria
            {
                PreparationGroupIds = [message.Id],
                IncludePreparationGroups = true,
                IncludePreparationGroupsItems = true,
                IncludeOrders = true,
            });
            var session = sessionQuery.Single();

            if (session.PreparationGroups!.SelectMany(pg => pg.PreparationGroupItems!).All(q => q.RemainingQuantity == 0) == false)
                return;

            List<Order> completedOrders = new List<Order>();
            foreach(var order in session.Orders!)
                if(order.State == OrderState.Processing)
                {
                    order.State = OrderState.Completed;
                    completedOrders.Add(order);
                }
            if (completedOrders.Any() == false)
                return;

            await repository.SaveChangesAsync();

            foreach (var order in completedOrders)
            {
                await eventService.Publish(new OnOrderCompletedEvent
                {
                    ChannelId = order.ChannelId,
                    MerchantId = order.MerchantId,
                    Id = order.Id,
                });

                await eventService.Publish(new OnOrderOperationEvent
                {
                    ChannelId = order.ChannelId,
                    MerchantId = order.MerchantId,
                    Id = order.Id,
                    Operation = EntityOperation.Update,
                });
            }
        }
    }
}
