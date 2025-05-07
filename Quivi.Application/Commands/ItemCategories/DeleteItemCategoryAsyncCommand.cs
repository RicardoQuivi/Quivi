using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.ItemCategories;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.ItemCategories
{
    public class DeleteItemCategoryAsyncCommand : ICommand<Task<IEnumerable<ItemCategory>>>
    {
        public required GetItemCategoriesCriteria Criteria { get; init; }
        public required Action<int> OnItemsAssociatedError { get; init; }
    }

    public class DeleteItemCategoryAsyncCommandHandler : ICommandHandler<DeleteItemCategoryAsyncCommand, Task<IEnumerable<ItemCategory>>>
    {
        private readonly IItemCategoriesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public DeleteItemCategoryAsyncCommandHandler(IItemCategoriesRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<ItemCategory>> Handle(DeleteItemCategoryAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IsDeleted = false,
                IncludeMenuItems = true,
            });
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var entitiesWithItems = new List<ItemCategory>();
            foreach (var e in entities)
            {
                if (e.MenuItemCategoryAssociations!.Any(c => c.MenuItem!.DeletedDate.HasValue == false))
                {
                    entitiesWithItems.Add(e);
                    break;
                }
                e.DeletedDate = now;
            }

            if (entitiesWithItems.Any())
            {
                foreach (var e in entitiesWithItems)
                    command.OnItemsAssociatedError(e.Id);
                return [];
            }

            await repository.SaveChangesAsync();

            foreach (var entity in entities)
                await eventService.Publish(new OnItemCategoryOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Delete,
                });

            return entities;
        }
    }
}