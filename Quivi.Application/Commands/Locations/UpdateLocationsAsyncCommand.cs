using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Locations;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Locations
{
    public interface IUpdatableLocation : IUpdatableEntity
    {
        int Id { get; }
        int MerchantId { get; }
        string Name { get; set; }
        bool IsDeleted { get; set; }
    }

    public class UpdateLocationsAsyncCommand : AUpdateAsyncCommand<IEnumerable<Location>, IUpdatableLocation>
    {
        public required GetLocationsCriteria Criteria { get; init; }
        public required Action<IUpdatableLocation> OnInvalidName { get; init; }
        public required Action<IUpdatableLocation> OnNameAlreadyExists { get; init; }
    }

    public class UpdateLocationsAsyncCommandHandler : ICommandHandler<UpdateLocationsAsyncCommand, Task<IEnumerable<Location>>>
    {
        private readonly ILocationsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdateLocationsAsyncCommandHandler(ILocationsRepository repository,
                                                IDateTimeProvider dateTimeProvider,
                                                IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<Location>> Handle(UpdateLocationsAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria);
            if (entities.Any() == false)
                return [];

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatableLocation> changedEntities = new();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableLocation(entity, now);
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
                await eventService.Publish(new OnLocationOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = entity.WasDeleted ? EntityOperation.Delete : EntityOperation.Update,
                });

            return entities;
        }

        private async Task<bool> IsNameValid(UpdateLocationsAsyncCommand command, IEnumerable<UpdatableLocation> updatedLocations)
        {
            var updatedLocationsWithNameChanges = updatedLocations.Where(n => n.NameChanged);

            bool hasInvalid = false;
            List<UpdatableLocation> entitiesWithValidName = [];
            foreach (var updatedLocation in updatedLocationsWithNameChanges)
            {
                if (string.IsNullOrWhiteSpace(updatedLocation.Name))
                {
                    command.OnInvalidName(updatedLocation);
                    hasInvalid = true;
                    continue;
                }

                entitiesWithValidName.Add(updatedLocation);
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

        private async Task<bool> ValidateDuplicateName(UpdateLocationsAsyncCommand command, int merchantId, IEnumerable<UpdatableLocation> entities)
        {
            bool hasInvalid = false;
            var updatedNamesDictionary = entities.ToDictionary(p => p.Id, p => p.Name);
            var locationsQuery = await repository.GetAsync(new GetLocationsCriteria
            {
                MerchantIds = [merchantId],
                Names = updatedNamesDictionary.Values,
                IsDeleted = false,
            });

            var locationsPerNameDictionary = locationsQuery.ToDictionary(p => p.Name, p => p);

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

        private class UpdatableLocation : IUpdatableLocation
        {
            public Location Model { get; }
            private readonly string originalName;
            private readonly bool originalIsDeleted;
            private readonly DateTime now;

            public UpdatableLocation(Location model, DateTime now)
            {
                Model = model;
                originalName = model.Name;
                originalIsDeleted = IsDeleted;
                this.now = now;
            }

            public int Id => this.Model.Id;
            public int MerchantId => this.Model.MerchantId;
            public string Name
            { 
                get => Model.Name;
                set => Model.Name = value;
            }
            public bool IsDeleted 
            {
                get => Model.DeletedDate.HasValue;
                set => Model.DeletedDate = value ? now : null;
            }

            public bool NameChanged => originalName != Model.Name;
            public bool DeletedChange => originalIsDeleted != IsDeleted;
            public bool WasDeleted => !originalIsDeleted && IsDeleted;
            public bool HasChanges => NameChanged || DeletedChange;
        }
    }
}