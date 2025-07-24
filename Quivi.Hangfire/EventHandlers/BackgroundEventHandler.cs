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

        public async Task Process(T message)
        {
            //try
            //{
            //    await Run(message);
            //}
            //catch
            //{
                backgroundJobHandler.Enqueue(() => Run(message));
            //}
        }

        public abstract Task Run(T message);
    }
}
