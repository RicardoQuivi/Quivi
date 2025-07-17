using Quivi.Application.Commands.Orders;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events.Data.Orders;
using Quivi.Infrastructure.Abstractions.Jobs;

namespace Quivi.Hangfire.EventHandlers.Orders
{
    public class OnOrderPendingApprovalEventHandler : BackgroundEventHandler<OnOrderPendingApprovalEvent>
    {
        private readonly ICommandProcessor commandProcessor;

        public OnOrderPendingApprovalEventHandler(ICommandProcessor commandProcessor,
                                                    IBackgroundJobHandler backgroundJobHandler) : base(backgroundJobHandler)
        {
            this.commandProcessor = commandProcessor;
        }

        public override Task Run(OnOrderPendingApprovalEvent message) => commandProcessor.Execute(new PrintOrderPendingApprovalAsyncCommand
        {
            OrderId = message.Id,
        });
    }
}