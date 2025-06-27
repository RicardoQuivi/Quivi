namespace Quivi.Backoffice.Api.Requests.Printers
{
    public class GetPrintersRequest : APagedRequest
    {
        public IEnumerable<string>? Ids { get; init; }
        public string? PrinterWorkerId { get; init; }
    }
}