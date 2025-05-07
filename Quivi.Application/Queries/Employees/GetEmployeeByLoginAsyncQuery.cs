using Microsoft.AspNet.Identity;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Queries.Employees
{
    public class GetEmployeeByLoginAsyncQuery : IQuery<Task<Employee?>>
    {
        public int MerchantId { get; init; }
        public int Id { get; init; }
        public required string PinCode { get; init; }
    }

    public class GetEmployeeByLoginQueryHandler : IQueryHandler<GetEmployeeByLoginAsyncQuery, Task<Employee?>>
    {
        private readonly IEmployeesRepository repository;
        private readonly IPasswordHasher passwordHasher;

        public GetEmployeeByLoginQueryHandler(IEmployeesRepository repository, IPasswordHasher passwordHasher)
        {
            this.repository = repository;
            this.passwordHasher = passwordHasher;
        }

        public async Task<Employee?> Handle(GetEmployeeByLoginAsyncQuery query)
        {
            var employeeQuery = await repository.GetAsync(new GetEmployeesCriteria
            {
                Ids = [query.Id],
                MerchantIds = [query.MerchantId],
                PageIndex = 0,
                PageSize = 1,
            });
            var employee = employeeQuery.SingleOrDefault();
            if (employee == null)
                return null;

            var state = passwordHasher.VerifyHashedPassword(employee.PinCodeHash, query.PinCode);
            if (state == PasswordVerificationResult.Failed)
                return null;

            if (state == PasswordVerificationResult.SuccessRehashNeeded)
            {
                employee.PinCodeHash = passwordHasher.HashPassword(employee.PinCodeHash);
                await repository.SaveChangesAsync();
            }
            return employee;
        }
    }
}
