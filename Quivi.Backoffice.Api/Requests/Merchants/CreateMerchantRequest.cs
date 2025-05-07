using Quivi.Domain.Entities.Merchants;

namespace Quivi.Backoffice.Api.Requests.Merchants
{
    public class PatchMerchantRequest : ARequest
    {
        public string? Name { get; set; }
        public string? Iban { get; set; }
        public string? VatNumber { get; set; }
        public decimal? VatRate { get; set; }
        public string? PostalCode { get; set; }
        public string? LogoUrl { get; set; }
        public decimal? TransactionFee { get; set; }
        public FeeUnit? TransactionFeeUnit { get; set; }
        public bool? AcceptTermsAndConditions { get; set; }
        public bool? IsDemo { get; set; }
        public bool? Inactive { get; set; }
    }
}