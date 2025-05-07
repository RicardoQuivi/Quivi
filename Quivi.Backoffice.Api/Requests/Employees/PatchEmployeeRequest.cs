using Quivi.Backoffice.Api.Dtos;
using Quivi.Infrastructure.Apis;

namespace Quivi.Backoffice.Api.Requests.Employees
{
    public class PatchEmployeeRequest : ARequest
    {
        public string? Name { get; init; }
        public Optional<TimeSpan> InactivityLogoutTimeout { get; init; }
        public IEnumerable<EmployeeRestriction>? Restrictions { get; set; }
    }
}