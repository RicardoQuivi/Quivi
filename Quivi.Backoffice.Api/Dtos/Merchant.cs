using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Quivi.Backoffice.Api.Dtos
{
    public class Merchant
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string VatNumber { get; init; }
        public required string LogoUrl { get; init; }
        public required decimal TransactionFee { get; init; }
        public required FeeUnit TransactionFeeUnit { get; init; }
        public required decimal SurchargeFee { get; init; }
        public required FeeUnit SurchargeFeeUnit { get; init; }
        public required decimal SetUpFee { get; init; }
        public required string? ParentId { get; init; }
        public required IDictionary<ChargeMethod, MerchantFee> SurchargeFees { get; init; }
    }

    public class MerchantFee
    {
        public decimal Fee { get; set; }
        public FeeUnit Unit { get; set; }
    }
}