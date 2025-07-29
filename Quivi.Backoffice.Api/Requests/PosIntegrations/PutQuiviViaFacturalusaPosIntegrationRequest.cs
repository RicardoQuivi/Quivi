namespace Quivi.Backoffice.Api.Requests.PosIntegrations
{
    public class PutQuiviViaFacturalusaPosIntegrationRequest
    {
        public required string AccessToken { get; set; }
        public bool SkipInvoice { get; set; }
        public bool IncludeTipInInvoice { get; set; }
        public string? InvoicePrefix { get; set; }
    }
}