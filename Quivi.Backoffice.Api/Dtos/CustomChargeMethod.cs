namespace Quivi.Backoffice.Api.Dtos
{
    public class CustomChargeMethod
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? LogoUrl { get; init; }
    }
}