namespace Quivi.Pos.Api.Dtos.Requests.Employees
{
    public class GetEmployeesRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; set; }
        public bool IncludeDeleted { get; set; }
    }
}