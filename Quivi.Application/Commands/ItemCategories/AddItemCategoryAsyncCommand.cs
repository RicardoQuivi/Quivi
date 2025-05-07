using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.ItemCategories;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.ItemCategories
{
    public class AddItemCategoryAsyncCommand : ICommand<Task<ItemCategory>>
    {
        public int MerchantId { get; init; }
        public required string Name { get; init; }
        public string? ImageUrl { get; init; }
        public IReadOnlyDictionary<Language, string>? Translations { get; init; }
    }

    public class AddItemCategoryAsyncCommandHandler : ICommandHandler<AddItemCategoryAsyncCommand, Task<ItemCategory>>
    {
        private readonly IItemCategoriesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddItemCategoryAsyncCommandHandler(IItemCategoriesRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<ItemCategory> Handle(AddItemCategoryAsyncCommand command)
        {
            var now = dateTimeProvider.GetUtcNow();
            var entity = new ItemCategory
            {
                MerchantId = command.MerchantId,
                Name = command.Name,
                ImagePath = command.ImageUrl,
                ItemCategoryTranslations = command.Translations?.Where(s => string.IsNullOrWhiteSpace(s.Value) == false).Select(s => new ItemCategoryTranslation
                {
                    Language = s.Key,
                    Name = s.Value,
                    CreatedDate = now,
                    ModifiedDate = now,
                }).ToList(),
                SortIndex = 0,
                CreatedDate = now,
                ModifiedDate = now,
                DeletedDate = null,
            };
            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnItemCategoryOperationEvent
            {
                Id = entity.Id,
                MerchantId = entity.MerchantId,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    }
}