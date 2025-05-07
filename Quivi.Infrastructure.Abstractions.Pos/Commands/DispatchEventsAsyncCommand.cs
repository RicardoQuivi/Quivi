using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;

namespace Quivi.Infrastructure.Abstractions.Pos.Commands
{
    public class DispatchEventsAsyncCommand : ICommand<Task>
    {
        public required IEnumerable<IEvent> Events { get; init; }
    }

    public class DispatchEventsAsyncCommandHandler : ICommandHandler<DispatchEventsAsyncCommand, Task>
    {
        private readonly IEventService eventService;

        public DispatchEventsAsyncCommandHandler(IEventService eventService)
        {
            this.eventService = eventService;
        }

        public async Task Handle(DispatchEventsAsyncCommand command)
        {
            foreach(var evt in command.Events)
                await eventService.Publish(evt);
        }
    }
}