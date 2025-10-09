using Quivi.Application.Commands.Availabilities;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.ChannelProfiles;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Hangfire.EventHandlers.ChannelProfiles
{
    public class OnChannelProfileOperationEventHandler : IEventHandler<OnChannelProfileOperationEvent>
    {
        private readonly ICommandProcessor commandProcessor;

        public OnChannelProfileOperationEventHandler(ICommandProcessor commandProcessor)
        {
            this.commandProcessor = commandProcessor;
        }

        public async Task Process(OnChannelProfileOperationEvent message)
        {
            await AssignNewEntitiesToAvailabilityGroups(message);
        }

        private async Task AssignNewEntitiesToAvailabilityGroups(OnChannelProfileOperationEvent message)
        {
            if (message.Operation != EntityOperation.Create)
                return;

            await commandProcessor.Execute(new UpdateAvailabilityGroupsAsyncCommand
            {
                Criteria = new GetAvailabilityGroupsCriteria
                {
                    MerchantIds = [message.MerchantId],
                    AutoAddNewChannelProfiles = true,
                    PageSize = null,
                },
                UpdateAction = group =>
                {
                    group.ChannelProfiles.Upsert(message.Id, t =>
                    {

                    });
                    return Task.CompletedTask;
                },
                OnInvalidName = _ => { },
            });
        }
    }
}
