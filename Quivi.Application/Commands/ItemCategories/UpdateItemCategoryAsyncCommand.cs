using Quivi.Domain.Entities;
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
    public interface IItemCategoryTranslation
    {
        Language Language { get; }
        string Name { get; set; }
    }

    public interface IUpdatableItemCategory : IUpdatableEntity
    {
        int MerchantId { get; }
        int Id { get; }
        string Name { get; set; }
        string? ImageUrl { get; set; }
        int SortIndex { get; set; }

        IUpdatableTranslations<IItemCategoryTranslation> Translations { get; }
    }


    public class UpdateItemCategoryAsyncCommand : AUpdateAsyncCommand<IEnumerable<ItemCategory>, IUpdatableItemCategory>
    {
        public required GetItemCategoriesCriteria Criteria { get; init; }
    }

    public class UpdateItemCategoryAsyncCommandHandler : ICommandHandler<UpdateItemCategoryAsyncCommand, Task<IEnumerable<ItemCategory>>>
    {
        private readonly IItemCategoriesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateItemCategoryAsyncCommandHandler(IItemCategoriesRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        private class UpdatableItemCategoryTranslation : IItemCategoryTranslation, IUpdatableEntity
        {
            public readonly ItemCategoryTranslation Model;
            private readonly bool isNew;
            private readonly string originalName;

            public UpdatableItemCategoryTranslation(ItemCategoryTranslation model)
            {
                Model = model;
                isNew = model.ItemCategoryId == 0;
                originalName = model.Name;
            }

            public Language Language => Model.Language;
            public string Name { get => Model.Name; set => Model.Name = value; }
            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    if (originalName != Model.Name)
                        return true;

                    return false;
                }
            }
        }

        private class UpdatableItemCategory : AUpdatableTranslatableEntity<ItemCategoryTranslation, UpdatableItemCategoryTranslation, IItemCategoryTranslation>, IUpdatableItemCategory
        {
            private readonly ItemCategory model;
            private readonly string originalName;
            private readonly string? originalImageUrl;
            private readonly int originalSortIndex;

            public UpdatableItemCategory(ItemCategory model, DateTime now) : base(model.ItemCategoryTranslations ?? [], t => new UpdatableItemCategoryTranslation(t), () => new ItemCategoryTranslation
            {
                Name = "",

                CreatedDate = now,
                ModifiedDate = now,
                DeletedDate = null,
            })
            {
                this.model = model;
                originalName = model.Name;
                originalImageUrl = model.ImagePath;
                originalSortIndex = model.SortIndex;
            }

            public int MerchantId => model.MerchantId;
            public int Id => model.Id;
            public string Name { get => model.Name; set => model.Name = value; }
            public string? ImageUrl { get => model.ImagePath; set => model.ImagePath = value; }
            public int SortIndex { get => model.SortIndex; set => model.SortIndex = value; }

            public override bool HasChanges
            {
                get
                {
                    if (originalName != model.Name)
                        return true;

                    if (originalImageUrl != model.ImagePath)
                        return true;

                    if (originalSortIndex != model.SortIndex)
                        return true;

                    return base.HasChanges;
                }
            }
        }

        public async Task<IEnumerable<ItemCategory>> Handle(UpdateItemCategoryAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeTranslations = true,
            });
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var changedEntities = new List<UpdatableItemCategory>();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableItemCategory(entity, now);
                await command.UpdateAction.Invoke(updatableEntity);

                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();
            foreach (var entity in changedEntities)
                await eventService.Publish(new OnItemCategoryOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Update,
                });

            return entities;
        }
    }
}
