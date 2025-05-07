using Microsoft.AspNet.Identity;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Employees;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Employees
{
    public interface IUpdatableEmployee : IUpdatableEntity
    {
        int Id { get; }
        int MerchantId { get; }
        string Name { get; set; }
        string? PinCode { set; }
        bool HasPinCode { get; }
        TimeSpan? InactivityLogoutTimeout { get; set; }
        EmployeeRestrictions Restrictions { get; set; }

        bool IsDeleted { get; set; }
    }

    public class UpdateEmployeesAsyncCommand : AUpdateAsyncCommand<IEnumerable<Employee>, IUpdatableEmployee>
    {
        public required GetEmployeesCriteria Criteria { get; init; }
        public required Action<IUpdatableEmployee> OnInvalidName { get; init; }
        public required Action<IUpdatableEmployee> OnNameAlreadyExists { get; init; }
    }

    public class UpdateEmployeesAsyncCommandHandler : ICommandHandler<UpdateEmployeesAsyncCommand, Task<IEnumerable<Employee>>>
    {
        private readonly IEmployeesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IPasswordHasher passwordHasher;
        private readonly IEventService eventService;

        public UpdateEmployeesAsyncCommandHandler(IEmployeesRepository repository,
                                                IDateTimeProvider dateTimeProvider,
                                                IPasswordHasher passwordHasher,
                                                IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.passwordHasher = passwordHasher;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<Employee>> Handle(UpdateEmployeesAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria);
            if (entities.Any() == false)
                return [];

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatableEmployee> changedEntities = new();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatableEmployee(entity, now, passwordHasher);
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
                await eventService.Publish(new OnEmployeeOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = entity.WasDeleted ? EntityOperation.Delete : EntityOperation.Update,
                });

            return entities;
        }

        private async Task<bool> IsNameValid(UpdateEmployeesAsyncCommand command, IEnumerable<UpdatableEmployee> updatedEmployees)
        {
            var updatedEmployeesWithNameChanges = updatedEmployees.Where(n => n.NameChanged);

            bool hasInvalid = false;
            List<UpdatableEmployee> entitiesWithValidName = [];
            foreach (var updatedEmployee in updatedEmployeesWithNameChanges)
            {
                if (string.IsNullOrWhiteSpace(updatedEmployee.Name))
                {
                    command.OnInvalidName(updatedEmployee);
                    hasInvalid = true;
                    continue;
                }

                entitiesWithValidName.Add(updatedEmployee);
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

        private async Task<bool> ValidateDuplicateName(UpdateEmployeesAsyncCommand command, int merchantId, IEnumerable<UpdatableEmployee> entities)
        {
            bool hasInvalid = false;
            var updatedNamesDictionary = entities.ToDictionary(p => p.Id, p => p.Name);
            var locationsQuery = await repository.GetAsync(new GetEmployeesCriteria
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

        private class UpdatableEmployee : IUpdatableEmployee
        {
            private Employee Model { get; }
            private readonly IPasswordHasher passwordHasher;
            private readonly string originalName;
            private readonly string? originalPinCodeHash;
            private readonly TimeSpan? originalInactivityLogoutTimeout;
            private readonly EmployeeRestrictions originalRestrictions;

            private readonly bool originalIsDeleted;
            private readonly DateTime now;
            public UpdatableEmployee(Employee model, DateTime now, IPasswordHasher passwordHasher)
            {
                this.Model = model;
                this.passwordHasher = passwordHasher;
                originalName = this.Model.Name;
                originalPinCodeHash = model.PinCodeHash;
                originalInactivityLogoutTimeout = model.LogoutInactivity;
                originalRestrictions = model.Restrictions;

                originalIsDeleted = IsDeleted;
                this.now = now;
            }

            public int Id => Model.Id;
            public int MerchantId => Model.MerchantId;
            public DateTime CreatedDate => Model.CreatedDate;
            public string Name { get => Model.Name; set => Model.Name = value; }
            public string? PinCode { set => Model.PinCodeHash = value == null ? null : passwordHasher.HashPassword(value); }
            public bool HasPinCode => Model.PinCodeHash != null;
            public TimeSpan? InactivityLogoutTimeout { get => Model.LogoutInactivity; set => Model.LogoutInactivity = value; }
            public EmployeeRestrictions Restrictions { get => Model.Restrictions; set => Model.Restrictions = value; }
            public bool IsDeleted
            {
                get => Model.DeletedDate.HasValue;
                set => Model.DeletedDate = value ? now : null;
            }

            public bool NameChanged => originalName != Model.Name;
            public bool WasDeleted => !originalIsDeleted && IsDeleted;

            public bool HasChanges
            {
                get
                {
                    if (originalName != Model.Name)
                        return true;

                    if (originalPinCodeHash != Model.PinCodeHash)
                        return true;

                    if (originalRestrictions != Model.Restrictions)
                        return true;

                    if (originalInactivityLogoutTimeout != Model.LogoutInactivity)
                        return true;

                    if (originalIsDeleted != IsDeleted)
                        return true;

                    return false;
                }
            }
        }
    }
}
