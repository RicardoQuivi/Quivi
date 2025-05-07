namespace Quivi.Backoffice.Api.Requests.Locals
{
    public class CreateLocalRequest : ARequest
    {
        public required string Name { get; init; } = string.Empty;
    }
}