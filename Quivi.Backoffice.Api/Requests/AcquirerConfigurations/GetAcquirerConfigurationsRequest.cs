namespace Quivi.Backoffice.Api.Requests.AcquirerConfigurations
{
    public class GetAcquirerConfigurationsRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
    }
}