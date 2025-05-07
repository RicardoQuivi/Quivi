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
    public class AddEmployeeAsyncCommand : ICommand<Task<Employee?>>
    {
        public int MerchantId { get; init; }
        public required string Name { get; init; }
        public TimeSpan? InactivityLogoutTimeout { get; init; }
        public EmployeeRestrictions Restrictions { get; init; }

        public required Action OnInvalidName { get; init; }
        public required Action OnNameAlreadyExists { get; init; }
    }

    public class AddEmployeeAsyncCommandHandler : ICommandHandler<AddEmployeeAsyncCommand, Task<Employee?>>
    {
        private readonly IEmployeesRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddEmployeeAsyncCommandHandler(IEmployeesRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<Employee?> Handle(AddEmployeeAsyncCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                command.OnInvalidName();
                return null;
            }

            var alreadyExistsQuery = await repository.GetAsync(new GetEmployeesCriteria
            {
                MerchantIds = [command.MerchantId],
                Names = [command.Name],
                PageSize = 0,
            });
            if (alreadyExistsQuery.TotalItems > 0)
            {
                command.OnNameAlreadyExists();
                return null;
            }

            var now = dateTimeProvider.GetUtcNow();
            var entity = new Employee
            {
                MerchantId = command.MerchantId,
                Name = command.Name,
                LogoutInactivity = command.InactivityLogoutTimeout,
                Restrictions = command.Restrictions,

                CreatedDate = now,
                ModifiedDate = now,
                DeletedDate = null,
            };
            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnEmployeeOperationEvent
            {
                Id = entity.Id,
                MerchantId = entity.MerchantId,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    };
}
