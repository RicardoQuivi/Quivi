using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Sessions;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Sessions
{
    public class CloseOpenedSessionsAsyncCommand : ICommand<Task>
    {
        public int PosIntegrationId { get; set; }
    }

    public class CloseOpenedSessionsAsyncCommandHandler : ICommandHandler<CloseOpenedSessionsAsyncCommand, Task>
    {
        private readonly ISessionsRepository repository;
        private readonly IEventService eventService;

        public CloseOpenedSessionsAsyncCommandHandler(ISessionsRepository repository, IEventService eventService)
        {
            this.repository = repository;
            this.eventService = eventService;
        }

        public async Task Handle(CloseOpenedSessionsAsyncCommand command)
        {
            var sessionsQuery = await repository.GetAsync(new GetSessionsCriteria
            {
                PosIntegrationIds = [command.PosIntegrationId],
                Statuses = [SessionStatus.Ordering],
                IncludeChannel = true,
            });

            if (sessionsQuery.Any() == false)
                return;

            foreach (var session in sessionsQuery)
                session.Status = SessionStatus.Unknown;

            await repository.SaveChangesAsync();

            foreach (var session in sessionsQuery)
                await eventService.Publish(new OnSessionOperationEvent
                {
                    ChannelId = session.ChannelId,
                    Id = session.Id,
                    MerchantId = session.Channel!.MerchantId,
                    Operation = EntityOperation.Delete,
                });
        }
    }
}