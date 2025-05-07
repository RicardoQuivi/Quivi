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
    public interface IUpdatableCustomChargeMethod : IUpdatableEntity
    {
        int Id { get; }
        int MerchantId { get; }
        string Name { get; set; }
        string? LogoUrl { get; set; }
    }

    public class UpdateCustomChargeMethodsAsyncCommand : AUpdateAsyncCommand<IEnumerable<CustomChargeMethod>, IUpdatableCustomChargeMethod>
    {
        public required GetCustomChargeMethodsCriteria Criteria { get; init; }
        public required Action<IUpdatableCustomChargeMethod> OnInvalidName { get; init; }
        public required Action<IUpdatableCustomChargeMethod> OnNameAlreadyExists { get; init; }
    }

    public class UpdateCustomChargeMethodsAsyncCommandHandler : ICommandHandler<UpdateCustomChargeMethodsAsyncCommand, Task<IEnumerable<CustomChargeMethod>>>
    {
        private readonly ICustomChargeMethodsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateCustomChargeMethodsAsyncCommandHandler(ICustomChargeMethodsRepository repository,
                                                IDateTimeProvider dateTimeProvider,
                                                IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<CustomChargeMethod>> Handle(UpdateCustomChargeMethodsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria);
            if (entities.Any() == false)
                return [];

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatableCustomChargeMethod> changedEntities = new();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableCustomChargeMethod(entity);
                await command.UpdateAction.Invoke(updatableEntity);
                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            if (await IsNameValid(command, changedEntities) == false)
                return [];

            await repository.SaveChangesAsync();

            foreach (var entity in changedEntities)
                await eventService.Publish(new OnCustomChargeMethodOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = EntityOperation.Update,
                });

            return entities;
        }

        private async Task<bool> IsNameValid(UpdateCustomChargeMethodsAsyncCommand command, IEnumerable<UpdatableCustomChargeMethod> updatedCustomChargeMethods)
        {
            var updatedCustomChargeMethodsWithNameChanges = updatedCustomChargeMethods.Where(n => n.NameChanged);

            bool hasInvalid = false;
            List<UpdatableCustomChargeMethod> entitiesWithValidName = [];
            foreach (var updatedCustomChargeMethod in updatedCustomChargeMethodsWithNameChanges)
            {
                if (string.IsNullOrWhiteSpace(updatedCustomChargeMethod.Name))
                {
                    command.OnInvalidName(updatedCustomChargeMethod);
                    hasInvalid = true;
                    continue;
                }

                entitiesWithValidName.Add(updatedCustomChargeMethod);
            }

            if (hasInvalid)
                return false;

            if (entitiesWithValidName.Any() == false)
                return true;

            var employeesPerMerchant = entitiesWithValidName.GroupBy(g => g.MerchantId);
            foreach (var e in employeesPerMerchant)
            {
                if (await ValidateDuplicateName(command, e.Key, e) == false)
                    continue;

                hasInvalid = true;
            }
            return hasInvalid;
        }

        private async Task<bool> ValidateDuplicateName(UpdateCustomChargeMethodsAsyncCommand command, int merchantId, IEnumerable<UpdatableCustomChargeMethod> entities)
        {
            bool hasInvalid = false;
            var updatedNamesDictionary = entities.ToDictionary(p => p.Id, p => p.Name);
            var customChargeMethodsQuery = await repository.GetAsync(new GetCustomChargeMethodsCriteria
            {
                MerchantIds = [merchantId],
                Names = updatedNamesDictionary.Values,
            });

            var locationsPerNameDictionary = customChargeMethodsQuery.ToDictionary(p => p.Name, p => p);

            foreach (var updated in entities)
            {
                if (locationsPerNameDictionary.TryGetValue(updated.Name, out var location) == false)
                    continue;

                if (location.Id == updated.Id)
                    continue;

                command.OnNameAlreadyExists(updated);
                hasInvalid = true;
            }
            return !hasInvalid;
        }

        private class UpdatableCustomChargeMethod : IUpdatableCustomChargeMethod
        {
            public CustomChargeMethod Model { get; }
            private readonly string originalName;
            private readonly string? originalLogoUrl;

            public UpdatableCustomChargeMethod(CustomChargeMethod model)
            {
                Model = model;
                originalName = model.Name;
                originalLogoUrl = model.Logo;
            }

            public int Id => this.Model.Id;
            public int MerchantId => this.Model.MerchantId;
            public string Name
            { 
                get => Model.Name;
                set => Model.Name = value;
            }
            public string? LogoUrl 
            {
                get => Model.Logo;
                set => Model.Logo = value;
            }

            public bool NameChanged => originalName != Model.Name;
            public bool LogoUrlChanged => originalLogoUrl != Model.Logo;
            public bool HasChanges => NameChanged || LogoUrlChanged;
        }
    }
}