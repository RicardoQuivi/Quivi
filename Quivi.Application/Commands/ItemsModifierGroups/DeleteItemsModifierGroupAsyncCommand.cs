using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.ItemsModifierGroups;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.ItemsModifierGroups
{
    public class DeleteItemsModifierGroupAsyncCommand : ICommand<Task<IEnumerable<ItemsModifierGroup>>>
    {
        public required GetItemsModifierGroupsCriteria Criteria { get; init; }
    }

    public class DeleteItemsModifierGroupAsyncCommandHandler : ICommandHandler<DeleteItemsModifierGroupAsyncCommand, Task<IEnumerable<ItemsModifierGroup>>>
    {
        private readonly IItemsModifierGroupsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public DeleteItemsModifierGroupAsyncCommandHandler(IItemsModifierGroupsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<ItemsModifierGroup>> Handle(DeleteItemsModifierGroupAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IsDeleted = false,
            });
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            foreach (var e in entities)
                e.DeletedDate = now;

            await repository.SaveChangesAsync();

            foreach (var entity in entities)
                await eventService.Publish(new OnItemsModifierGroupOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Delete,
                });

            return entities;
        }
    }
}
