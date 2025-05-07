namespace Quivi.Backoffice.Api.Requests.Merchants
{
    public class CreateMerchantRequest : ARequest
    {
        public required string FiscalName { get; init; }
        public required string Name { get; init; }
        public required string PostalCode { get; init; }
        public required string VatNumber { get; init; }
        public required string Iban { get; init; }
        public required string IbanProofUrl { get; init; }
        public required string LogoUrl { get; init; }
    }
}
