using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Jobs;

namespace Quivi.Hangfire.EventHandlers
{
    public abstract class BackgroundEventHandler<T> : IEventHandler<T> where T : IEvent
    {
        protected readonly IBackgroundJobHandler backgroundJobHandler;

        public BackgroundEventHandler(IBackgroundJobHandler backgroundJobHandler)
        {
            this.backgroundJobHandler = backgroundJobHandler;
        }

        public Task Process(T message)
        {
            backgroundJobHandler.Enqueue(() => Run(message));
            return Task.CompletedTask;
        }

        public abstract Task Run(T message);
    }
}
