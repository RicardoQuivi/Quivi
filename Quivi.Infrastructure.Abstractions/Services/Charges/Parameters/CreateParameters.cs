using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Infrastructure.Abstractions.Services.Charges.Parameters
{
    public class CreateParameters
    {
        public class PayAtTheTable
        {
            public int SessionId { get; init; }
            public IEnumerable<SessionItem>? Items { get; init; }
        }

        public class OrderAndPay
        {
            public int OrderId { get; init; }
        }

        public int ChannelId { get; init; }
        public int MerchantAcquirerConfigurationId { get; init; }
        public int? ConsumerPersonId { get; init; }
        public string? VatNumber { get; init; }
        public string? Email { get; init; }
        public decimal Amount { get; init; }
        public decimal Tip { get; init; }
        public PayAtTheTable? PayAtTheTableData { get; init; }
        public OrderAndPay? OrderAndPayData { get; init; }


        public ChargeMethod? SurchargeFeeOverride { get; init; }
        public required string UserLanguageIso { get; init; }

        public required Action OnInvalidAdditionalData { get; init; }
        public required Action OnInvalidTip { get; init; }
        public required Action OnInvalidAmount { get; init; }
        public required Action OnInvalidChannel { get; init; }
        public required Action OnInvalidSession { get; init; }
        public required Action OnInvalidMerchantAcquirerConfiguration { get; init; }
        public required Action OnNoOpenSession { get; init; }
    }
}