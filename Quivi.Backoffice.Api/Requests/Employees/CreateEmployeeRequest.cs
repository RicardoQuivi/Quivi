using Quivi.Backoffice.Api.Dtos;

namespace Quivi.Backoffice.Api.Requests.Employees
{
    public class CreateEmployeeRequest : ARequest
    {
        public required string Name { get; init; } = string.Empty;
        public TimeSpan? InactivityLogoutTimeout { get; init; }
        public required IEnumerable<EmployeeRestriction> Restrictions { get; init; } = [];
    } 
}
