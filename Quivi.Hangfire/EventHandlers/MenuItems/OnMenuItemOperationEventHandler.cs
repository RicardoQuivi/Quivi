using Quivi.Application.Commands.Availabilities;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.MenuItems;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Hangfire.EventHandlers.MenuItems
{
    public class OnMenuItemOperationEventHandler : IEventHandler<OnMenuItemOperationEvent>
    {
        private readonly ICommandProcessor commandProcessor;

        public OnMenuItemOperationEventHandler(ICommandProcessor commandProcessor)
        {
            this.commandProcessor = commandProcessor;
        }

        public async Task Process(OnMenuItemOperationEvent message)
        {
            await AssignNewEntitiesToAvailabilityGroups(message);
        }

        private async Task AssignNewEntitiesToAvailabilityGroups(OnMenuItemOperationEvent message)
        {
            if (message.Operation != EntityOperation.Create)
                return;

            await commandProcessor.Execute(new UpdateAvailabilityGroupsAsyncCommand
            {
                Criteria = new GetAvailabilityGroupsCriteria
                {
                    MerchantIds = [message.MerchantId],
                    AutoAddNewMenuItems = true,
                    PageSize = null,
                },
                UpdateAction = group =>
                {
                    group.MenuItems.Upsert(message.Id, t =>
                    {

                    });
                    return Task.CompletedTask;
                },
                OnInvalidName = _ => { },
            });
        }
    }
}