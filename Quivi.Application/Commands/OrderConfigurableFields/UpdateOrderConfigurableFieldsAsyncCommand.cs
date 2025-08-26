using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.OrderConfigurableFieldChannelProfileAssociations;
using Quivi.Infrastructure.Abstractions.Events.Data.OrderConfigurableFields;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.OrderConfigurableFields
{
    public interface IUpdatableOrderConfigurableFieldTranslation
    {
        Language Language { get; }
        string Name { get; set; }
    }

    public interface IUpdatableChannelProfileAssociation : IUpdatableEntity
    {
        public int Id { get; }
    }

    public interface IUpdatableOrderConfigurableField : IUpdatableEntity
    {
        int Id { get; }
        int MerchantId { get; }
        string Name { get; set; }
        FieldType Type { get; set; }
        bool IsRequired { get; set; }
        bool IsAutoFill { get; set; }
        PrintedOn PrintedOn { get; set; }
        AssignedOn AssignedOn { get; set; }
        string? DefaultValue { get; set; }
        bool IsDeleted { get; set; }

        IUpdatableTranslations<IUpdatableOrderConfigurableFieldTranslation> Translations { get; }
        IUpdatableRelationship<IUpdatableChannelProfileAssociation, int> ChannelProfiles { get; }

    }

    public class UpdateOrderConfigurableFieldsAsyncCommand : AUpdateAsyncCommand<IEnumerable<OrderConfigurableField>, IUpdatableOrderConfigurableField>
    {
        public required GetOrderConfigurableFieldsCriteria Criteria { get; init; }
        public required Action OnAutoFillWithEmptyDefaultValue { get; set; }
        public required Action OnInvalidDefaultValue { get; set; }
    }

    public class UpdateOrderConfigurableFieldsAsyncCommandHandler : ICommandHandler<UpdateOrderConfigurableFieldsAsyncCommand, Task<IEnumerable<OrderConfigurableField>>>
    {
        private readonly IOrderConfigurableFieldsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateOrderConfigurableFieldsAsyncCommandHandler(IOrderConfigurableFieldsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        private class UpdatableEatsOrderConfigurableFieldTranslation : IUpdatableOrderConfigurableFieldTranslation, IUpdatableEntity
        {
            public readonly OrderConfigurableFieldTranslation Model;
            private readonly bool isNew;
            private readonly string originalName;

            public UpdatableEatsOrderConfigurableFieldTranslation(OrderConfigurableFieldTranslation model)
            {
                Model = model;
                isNew = model.OrderConfigurableFieldId == 0;
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

        private class UpdatableChannelProfileAssociation : IUpdatableChannelProfileAssociation
        {
            public readonly OrderConfigurableFieldChannelProfileAssociation Model;
            private readonly bool isNew;

            public UpdatableChannelProfileAssociation(OrderConfigurableFieldChannelProfileAssociation model)
            {
                this.Model = model;
                isNew = model.OrderConfigurableFieldId == 0;
            }

            public int Id => Model.ChannelProfileId;

            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    return false;
                }
            }
        }

        private class UpdatableOrderConfigurableField : AUpdatableTranslatableEntity<OrderConfigurableFieldTranslation, UpdatableEatsOrderConfigurableFieldTranslation,
                                                                IUpdatableOrderConfigurableFieldTranslation>,
                                                                IUpdatableOrderConfigurableField
        {
            private readonly OrderConfigurableField model;
            public readonly UpdatableRelationshipEntity<OrderConfigurableFieldChannelProfileAssociation, IUpdatableChannelProfileAssociation, int> UpdatableAssociations;
            private readonly string originalName;
            private readonly FieldType originalType;
            private readonly bool originalIsRequired;
            private readonly bool originalIsAutoFill;
            private readonly PrintedOn originalPrintedOn;
            private readonly AssignedOn originalAssignedOn;
            private readonly string? originalDefaultValue;
            private readonly bool originalIsDeleted;
            private readonly DateTime now;

            public UpdatableOrderConfigurableField(OrderConfigurableField model, DateTime now) : base(model.Translations ?? [], t => new UpdatableEatsOrderConfigurableFieldTranslation(t), () => new OrderConfigurableFieldTranslation
            {
                Name = "",

                CreatedDate = now,
                ModifiedDate = now,
                DeletedDate = null,
            })
            {
                this.model = model;
                this.now = now;
                this.UpdatableAssociations = new UpdatableRelationshipEntity<OrderConfigurableFieldChannelProfileAssociation, IUpdatableChannelProfileAssociation, int>(model.AssociatedChannelProfiles!, m => m.ChannelProfileId, t => new UpdatableChannelProfileAssociation(t), (id) => new OrderConfigurableFieldChannelProfileAssociation
                {
                    ChannelProfileId = id,
                    OrderConfigurableField = this.model,
                    OrderConfigurableFieldId = this.model.Id,

                    CreatedDate = now,
                    ModifiedDate = now,
                });

                originalName = model.Name;
                originalType = model.Type;
                originalIsRequired = model.IsRequired;
                originalIsAutoFill = model.IsAutoFill;
                originalPrintedOn = model.PrintedOn;
                originalAssignedOn = model.AssignedOn;
                originalDefaultValue = model.DefaultValue;
                originalIsDeleted = model.DeletedDate.HasValue;
            }

            public int Id => model.Id;
            public int MerchantId => model.MerchantId;
            public string Name { get => model.Name; set => model.Name = value; }
            public FieldType Type { get => model.Type; set => model.Type = value; }
            public bool IsRequired { get => model.IsRequired; set => model.IsRequired = value; }
            public bool IsAutoFill { get => model.IsAutoFill; set => model.IsAutoFill = value; }
            public PrintedOn PrintedOn { get => model.PrintedOn; set => model.PrintedOn = value; }
            public AssignedOn AssignedOn { get => model.AssignedOn; set => model.AssignedOn = value; }
            public string? DefaultValue { get => model.DefaultValue; set => model.DefaultValue = value; }
            public bool IsDeleted { get => model.DeletedDate.HasValue; set => model.DeletedDate = value ? now : null; }
            public bool WasDeleted => originalIsDeleted == false && IsDeleted;
            public IUpdatableRelationship<IUpdatableChannelProfileAssociation, int> ChannelProfiles => UpdatableAssociations;


            public override bool HasChanges
            {
                get
                {
                    if (originalName != model.Name)
                        return true;

                    if (originalType != model.Type)
                        return true;

                    if (originalIsRequired != model.IsRequired)
                        return true;

                    if (originalIsAutoFill != model.IsAutoFill)
                        return true;

                    if (originalPrintedOn != model.PrintedOn)
                        return true;

                    if (originalAssignedOn != model.AssignedOn)
                        return true;

                    if (originalDefaultValue != model.DefaultValue)
                        return true;

                    if (originalIsDeleted != IsDeleted)
                        return true;

                    if (UpdatableAssociations.HasChanges)
                        return true;

                    return base.HasChanges;
                }
            }

        }


        public async Task<IEnumerable<OrderConfigurableField>> Handle(UpdateOrderConfigurableFieldsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria with
            {
                IncludeTranslations = true,
                IncludeChannelProfileAssociations = true,
            });
            if (!entities.Any())
                return entities;

            var now = dateTimeProvider.GetUtcNow();
            var changedEntities = new List<UpdatableOrderConfigurableField>();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableOrderConfigurableField(entity, now);
                await command.UpdateAction.Invoke(updatableEntity);

                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            bool hasError = false;
            foreach (var changedEntity in changedEntities)
            {
                if (changedEntity.IsAutoFill)
                {
                    if (string.IsNullOrWhiteSpace(changedEntity.DefaultValue))
                    {
                        command.OnAutoFillWithEmptyDefaultValue.Invoke();
                        hasError = true;
                    }
                }

                if (changedEntity.Type == FieldType.NumbersOnly)
                {
                    if (string.IsNullOrWhiteSpace(changedEntity.DefaultValue) == false && decimal.TryParse(changedEntity.DefaultValue, out var number) == false)
                    {
                        command.OnInvalidDefaultValue.Invoke();
                        hasError = true;
                    }
                }

                if (changedEntity.Type == FieldType.Boolean)
                {
                    if (string.IsNullOrWhiteSpace(changedEntity.DefaultValue) == false && new[] { "0", "1", "true", "false" }.Contains(changedEntity.DefaultValue.ToLower()) == false)
                    {
                        command.OnInvalidDefaultValue.Invoke();
                        hasError = true;
                    }
                }
            }
            if (hasError)
                return [];

            await repository.SaveChangesAsync();
            foreach (var entity in changedEntities)
            {
                await eventService.Publish(new OnOrderConfigurableFieldOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = entity.WasDeleted ? EntityOperation.Delete : EntityOperation.Update,
                });

                foreach (var changedEntity in entity.UpdatableAssociations.ChangedEntities)
                    await eventService.Publish(new OnOrderConfigurableFieldChannelProfileAssociationOperationEvent
                    {
                        MerchantId = entity.MerchantId,
                        OrderConfigurableFieldId = changedEntity.Entity.OrderConfigurableFieldId,
                        ChannelProfileId = changedEntity.Entity.ChannelProfileId,
                        Operation = changedEntity.Operation,
                    });
            }

            return entities;
        }
    }
}