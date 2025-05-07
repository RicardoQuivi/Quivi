namespace Quivi.Backoffice.Api.Requests.Employees
{
    public class GetEmployeesRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}
