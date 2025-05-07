using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;

namespace Quivi.Infrastructure.Abstractions.Pos.Commands
{
    public abstract class ASyncCommandHandler<TCommand> : ICommandHandler<TCommand, Task<IEnumerable<IEvent>>> where TCommand : ICommand<Task<IEnumerable<IEvent>>>
    {
        private readonly Dictionary<Order, List<Func<Order, IEvent>>> orderEvents = new();
        private readonly Dictionary<Session, List<Func<Session, IEvent>>> sessionEvents = new();

        public async Task<IEnumerable<IEvent>> Handle(TCommand command)
        {
            await Sync(command);

            List<IEvent> result = new();
            foreach(var (order, generators) in orderEvents)
                foreach(var generator in generators)
                    result.Add(generator(order));

            return result.Distinct().ToList();
        }

        protected abstract Task Sync(TCommand command);

        protected void AddOrderEvent(Order order, Func<Order, IEvent> eventGenerator)
        {
            if(orderEvents.TryGetValue(order, out var events))
            {
                events.Add(eventGenerator);
                return;
            }

            orderEvents.Add(order, new List<Func<Order, IEvent>>
            {
                eventGenerator,
            });
        }

        protected void AddSessionEvent(Session session, Func<Session, IEvent> eventGenerator)
        {
            if (sessionEvents.TryGetValue(session, out var events))
            {
                events.Add(eventGenerator);
                return;
            }

            sessionEvents.Add(session, new List<Func<Session, IEvent>>
            {
                eventGenerator,
            });
        }
    }
}
