using Quivi.Application.Commands.Payouts;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.Settlements;

namespace Quivi.Hangfire.EventHandlers.Settlements
{
    public class OnSettlementFinishedEventHandler : IEventHandler<OnSettlementFinishedEvent>
    {
        private readonly ICommandProcessor commandProcessor;

        public OnSettlementFinishedEventHandler(ICommandProcessor commandProcessor)
        {
            this.commandProcessor = commandProcessor;
        }

        public Task Process(OnSettlementFinishedEvent message) => commandProcessor.Execute(new ProcessPayoutsAsyncCommand
        {
            SettlementId = message.Id,
        });
    }
}