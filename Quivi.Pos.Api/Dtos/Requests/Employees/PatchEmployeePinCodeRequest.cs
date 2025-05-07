namespace Quivi.Pos.Api.Dtos.Requests.Employees
{
    public class PatchEmployeePinCodeRequest
    {
        public required string PinCode { get; init; } = string.Empty;
    }
}
