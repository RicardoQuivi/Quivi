namespace Quivi.Backoffice.Api.Requests.AcquirerConfigurations
{
    public class UpsertPaybyrdAcquirerConfigurationRequest : UpsertAcquirerConfigurationRequest
    {
        public string? ApiKey { get; init; }
    }
}