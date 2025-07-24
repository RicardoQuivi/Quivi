using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;

namespace Quivi.Backoffice.Api.Requests.Merchants
{
    public class PatchMerchantRequest : ARequest
    {
        public string? Name { get; init; }
        public string? Iban { get; init; }
        public string? VatNumber { get; init; }
        public decimal? VatRate { get; init; }
        public string? PostalCode { get; init; }
        public string? LogoUrl { get; init; }
        public decimal? TransactionFee { get; init; }
        public FeeUnit? TransactionFeeUnit { get; init; }
        public decimal? SurchargeFee { get; init; }
        public FeeUnit? SurchargeFeeUnit { get; init; }
        public bool? AcceptTermsAndConditions { get; init; }
        public bool? IsDemo { get; init; }
        public bool? Inactive { get; init; }
        public IDictionary<ChargeMethod, PatchFee>? SurchargeFees { get; init; }
    }

    public class PatchFee
    {
        public decimal? Fee { get; init; }
        public FeeUnit? Unit { get; init; }
    }
}