using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.MenuItems;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.MenuItems
{
    public class AddMenuItemTranslation
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
    }

    public class AddMenuItemAsyncCommand : ICommand<Task<MenuItem>>
    {
        public int MerchantId { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public decimal Price { get; init; }
        public PriceType PriceType { get; init; }
        public decimal VatRate { get; init; }
        public int? LocationId { get; init; }
        public required IEnumerable<int> MenuItemCategoryIds { get; init; }
        public IReadOnlyDictionary<Language, AddMenuItemTranslation>? Translations { get; init; }
    }

    public class AddMenuItemAsyncCommandHandler : ICommandHandler<AddMenuItemAsyncCommand, Task<MenuItem>>
    {
        private readonly IMenuItemsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddMenuItemAsyncCommandHandler(IMenuItemsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<MenuItem> Handle(AddMenuItemAsyncCommand command)
        {
            var now = dateTimeProvider.GetUtcNow();
            var entity = new MenuItem
            {
                MerchantId = command.MerchantId,
                Name = command.Name,
                Description = command.Description,
                Price = command.Price,
                PriceType = command.PriceType,
                VatRate = command.VatRate,
                MenuItemTranslations = command.Translations?.Select(s => new MenuItemTranslation
                {
                    Name = s.Value.Name,
                    Description = s.Value.Description,
                    Language = s.Key,

                    CreatedDate = now,
                    ModifiedDate = now,
                    DeletedDate = null,
                }).ToList() ?? [],
                MenuItemCategoryAssociations = command.MenuItemCategoryIds?.Select((s, i) => new MenuItemCategoryAssociation
                {
                    ItemCategoryId = s,
                    SortIndex = i,

                    CreatedDate = now,
                    ModifiedDate = now,
                }).ToList(),
                ImageUrl = command.ImageUrl,
                LocationId = command.LocationId,
                Stock = false,

                CreatedDate = now,
                ModifiedDate = now,
                DeletedDate = null,
            };
            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnMenuItemOperationEvent
            {
                Id = entity.Id,
                MerchantId = entity.MerchantId,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    }
}