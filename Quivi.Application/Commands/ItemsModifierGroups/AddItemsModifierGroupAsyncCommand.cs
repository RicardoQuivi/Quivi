using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.ItemsModifierGroups;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.ItemsModifierGroups
{
    public class AddItemsModifierGroupTranslation
    {
        public required string Name { get; init; }
    }

    public class AddModifierItem
    {
        public decimal Price { get; init; }
        public int SortIndex { get; init; }
    }

    public class AddItemsModifierGroupAsyncCommand : ICommand<Task<ItemsModifierGroup>>
    {
        public int MerchantId { get; init; }
        public required string Name { get; init; }
        public int MinSelection { get; init; }
        public int MaxSelection { get; init; }
        public IReadOnlyDictionary<int, AddModifierItem>? Items { get; init; }
        public IReadOnlyDictionary<Language, AddItemsModifierGroupTranslation>? Translations { get; init; }
    }

    public class AddItemsModifierGroupAsyncCommandHandler : ICommandHandler<AddItemsModifierGroupAsyncCommand, Task<ItemsModifierGroup>>
    {
        private readonly IItemsModifierGroupsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddItemsModifierGroupAsyncCommandHandler(IItemsModifierGroupsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<ItemsModifierGroup> Handle(AddItemsModifierGroupAsyncCommand command)
        {
            var now = dateTimeProvider.GetUtcNow();
            var entity = new ItemsModifierGroup
            {
                MerchantId = command.MerchantId,
                Name = command.Name,

                MinSelection = command.MinSelection,
                MaxSelection = command.MaxSelection,

                MenuItemModifiers = command.Items?.Select(entry => new MenuItemModifier
                {
                    MenuItemId = entry.Key,
                    Price = entry.Value.Price,
                    SortIndex = entry.Value.SortIndex,

                    CreatedDate = now,
                    ModifiedDate = now,
                    DeletedDate = null,
                }).ToList() ?? [],

                ItemsModifierGroupTranslations = command.Translations?.Select(s => new ItemsModifierGroupTranslation
                {
                    Name = s.Value.Name,
                    Language = s.Key,

                    CreatedDate = now,
                    ModifiedDate = now,
                    DeletedDate = null,
                }).ToList() ?? [],

                CreatedDate = now,
                ModifiedDate = now,
                DeletedDate = null,
            };
            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnItemsModifierGroupOperationEvent
            {
                Id = entity.Id,
                MerchantId = entity.MerchantId,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    }
}
