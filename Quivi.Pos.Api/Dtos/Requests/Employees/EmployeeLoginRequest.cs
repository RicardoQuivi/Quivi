namespace Quivi.Pos.Api.Dtos.Requests.Employees
{
    public class EmployeeLoginRequest : ARequest
    {
        public required string PinCode { get; init; } = string.Empty;
    }
}
