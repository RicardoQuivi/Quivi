namespace Quivi.Backoffice.Api.Requests.AcquirerConfigurations
{
    public class UpsertPaybyrdTerminalAcquirerConfigurationRequest : UpsertPaybyrdAcquirerConfigurationRequest
    {
        public string? TerminalId { get; init; }
    }
}