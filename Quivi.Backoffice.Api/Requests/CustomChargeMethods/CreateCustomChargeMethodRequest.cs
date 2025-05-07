namespace Quivi.Backoffice.Api.Requests.CustomChargeMethods
{
    public class CreateCustomChargeMethodRequest : ARequest
    {
        public required string Name { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
    }
}