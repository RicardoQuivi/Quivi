using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PosIntegrations;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.PosIntegrations
{
    public class AddPosIntegrationAsyncCommand : ICommand<Task<PosIntegration>>
    {
        public int MerchantId { get; set; }
        public IntegrationType IntegrationType { get; set; }
        public required string ConnectionString { get; set; }
        public bool DiagnosticErrorsMuted { get; set; }
    }

    public class AddPosIntegrationAsyncCommandHandler : ICommandHandler<AddPosIntegrationAsyncCommand, Task<PosIntegration>>
    {
        private readonly IPosIntegrationsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddPosIntegrationAsyncCommandHandler(IPosIntegrationsRepository repository, IEventService eventService, IDateTimeProvider dateTimeProvider)
        {
            this.repository = repository;
            this.eventService = eventService;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<PosIntegration> Handle(AddPosIntegrationAsyncCommand command)
        {
            var now = dateTimeProvider.GetUtcNow();
            var integration = new PosIntegration
            {
                MerchantId = command.MerchantId,
                IntegrationType = command.IntegrationType,
                ConnectionString = command.ConnectionString,
                DiagnosticErrorsMuted = command.DiagnosticErrorsMuted,
                SyncState = SyncState.Unknown,
                CreatedDate = now,
                ModifiedDate = now,
            };

            repository.Add(integration);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnPosIntegrationOperationEvent
            {
                Id = integration.Id,
                MerchantId = integration.MerchantId,
                Operation = EntityOperation.Create,
            });
            return integration;
        }
    }
}
