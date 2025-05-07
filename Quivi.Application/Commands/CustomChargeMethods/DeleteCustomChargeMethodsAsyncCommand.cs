using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.CustomChargeMethods;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.CustomChargeMethods
{
    public class DeleteCustomChargeMethodsAsyncCommand : ICommand<Task<IEnumerable<CustomChargeMethod>>>
    {
        public required GetCustomChargeMethodsCriteria Criteria { get; init; }
    }

    public class DeleteCustomChargeMethodsAsyncCommandHandler : ICommandHandler<DeleteCustomChargeMethodsAsyncCommand, Task<IEnumerable<CustomChargeMethod>>>
    {
        private readonly ICustomChargeMethodsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public DeleteCustomChargeMethodsAsyncCommandHandler(ICustomChargeMethodsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<CustomChargeMethod>> Handle(DeleteCustomChargeMethodsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria);
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            foreach (var e in entities)
                repository.Remove(e);

            await repository.SaveChangesAsync();

            foreach (var entity in entities)
                await eventService.Publish(new OnCustomChargeMethodOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Delete,
                });

            return entities;
        }
    }
}
