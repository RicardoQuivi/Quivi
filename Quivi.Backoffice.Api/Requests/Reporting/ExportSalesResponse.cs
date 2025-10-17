using Quivi.Backoffice.Api.Responses;

namespace Quivi.Backoffice.Api.Requests.Reporting
{
    public class ExportSalesResponse : AResponse<string>
    {
        public required string Name { get; init; }
    }
}
