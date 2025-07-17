namespace Quivi.Backoffice.Api.Requests.AcquirerConfigurations
{
    public abstract class UpsertAcquirerConfigurationRequest : ARequest
    {
        public bool IsActive { get; init; }
    }
}
