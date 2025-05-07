using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.MenuItems;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.MenuItems
{
    public class DeleteMenuItemsAsyncCommand : ICommand<Task<IEnumerable<MenuItem>>>
    {
        public required GetMenuItemsCriteria Criteria { get; init; }
        public required Action<int> OnItemsAssociatedWithModifiersError { get; init; }
    }

    public class DeleteMenuItemsAsyncCommandHandler : ICommandHandler<DeleteMenuItemsAsyncCommand, Task<IEnumerable<MenuItem>>>
    {
        private readonly IMenuItemsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public DeleteMenuItemsAsyncCommandHandler(IMenuItemsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<MenuItem>> Handle(DeleteMenuItemsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeMenuItemModifiers = true,
                IsDeleted = false,
            });
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var entitiesBelongingWithModifiers = new List<MenuItem>();
            foreach (var e in entities)
            {
                if (e.MenuItemModifiers!.Any(c => c.DeletedDate.HasValue == false))
                {
                    entitiesBelongingWithModifiers.Add(e);
                    break;
                }
                e.DeletedDate = now;
            }

            if (entitiesBelongingWithModifiers.Any())
            {
                foreach (var e in entitiesBelongingWithModifiers)
                    command.OnItemsAssociatedWithModifiersError(e.Id);
                return [];
            }

            await repository.SaveChangesAsync();

            foreach (var entity in entities)
                await eventService.Publish(new OnMenuItemOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Delete,
                });

            return entities;
        }
    }
}
