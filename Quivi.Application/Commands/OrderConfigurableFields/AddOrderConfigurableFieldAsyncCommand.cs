using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.OrderConfigurableFields;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Application.Commands.OrderConfigurableFields
{
    public class AddOrderConfigurableFieldAsyncCommand : ICommand<Task<OrderConfigurableField?>>
    {
        public int MerchantId { get; init; }
        public required string Name { get; init; }
        public FieldType FieldType { get; init; }
        public bool IsRequired { get; init; }
        public bool IsAutoFill { get; init; }
        public AssignedOn AssignedOn { get; init; }
        public PrintedOn PrintedOn { get; init; }
        public string? DefaultValue { get; init; }
        public IReadOnlyDictionary<Language, string>? Translations { get; init; }

        public required Action OnInvalidName { get; init; }
        public required Action OnAutoFillWithEmptyDefaultValue { get; init; }
        public required Action OnInvalidDefaultValue { get; init; }
    }

    public class AddOrderConfigurableFieldAsyncCommandHandler : ICommandHandler<AddOrderConfigurableFieldAsyncCommand, Task<OrderConfigurableField?>>
    {
        private readonly IOrderConfigurableFieldsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddOrderConfigurableFieldAsyncCommandHandler(IOrderConfigurableFieldsRepository repository,
                                                            IDateTimeProvider dateTimeProvider,
                                                            IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<OrderConfigurableField?> Handle(AddOrderConfigurableFieldAsyncCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                command.OnInvalidName.Invoke();
                return null;
            }

            if (command.IsAutoFill)
            {
                if (string.IsNullOrWhiteSpace(command.DefaultValue))
                {
                    command.OnAutoFillWithEmptyDefaultValue.Invoke();
                    return null;
                }
            }

            if (command.FieldType == FieldType.NumbersOnly)
            {
                if (string.IsNullOrWhiteSpace(command.DefaultValue) == false && decimal.TryParse(command.DefaultValue, out var number) == false)
                {
                    command.OnInvalidDefaultValue.Invoke();
                    return null;
                }
            }

            if (command.FieldType == FieldType.Boolean)
            {
                if (string.IsNullOrWhiteSpace(command.DefaultValue) == false && new[] { "0", "1", "true", "false" }.Contains(command.DefaultValue.ToLower()) == false)
                {
                    command.OnInvalidDefaultValue.Invoke();
                    return null;
                }
            }

            var now = dateTimeProvider.GetUtcNow();
            var entity = new OrderConfigurableField
            {
                MerchantId = command.MerchantId,

                Name = command.Name,
                Type = command.FieldType,
                IsRequired = command.IsRequired,
                PrintedOn = command.PrintedOn,
                AssignedOn = command.AssignedOn,
                DefaultValue = command.DefaultValue,
                IsAutoFill = command.IsAutoFill,

                Translations = command.Translations?.Select(t => new OrderConfigurableFieldTranslation
                {
                    Language = t.Key,
                    Name = t.Value,

                    CreatedDate = now,
                    ModifiedDate = now,
                }).ToList(),

                CreatedDate = now,
                ModifiedDate = now,
            };

            this.repository.Add(entity);
            await this.repository.SaveChangesAsync();

            await eventService.Publish(new OnOrderConfigurableFieldOperationEvent
            {
                MerchantId = entity.MerchantId,
                Id = entity.Id,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    }
}