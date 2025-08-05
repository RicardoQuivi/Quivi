using Quivi.Domain.Entities.Charges;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetChargesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<ChargeStatus>? Statuses { get; init; }
        public IEnumerable<ChargeMethod>? Methods { get; init; }

        public bool IncludeDeposit { get; init; }
        public bool IncludeDepositDepositCapture { get; init; }
        public bool IncludeDepositCaptureJournal { get; init; }
        public bool IncludeDepositCaptureJournalPostings { get; init; }
        public bool IncludeDepositCaptureJournalPostingsPerson { get; init; }
        public bool IncludeDepositCaptureJournalChanges { get; init; }
        public bool IncludeDepositDepositSurchage { get; init; }
        public bool IncludeDepositDepositSurcharge { get; init; }
        public bool IncludeDepositSurchargeJournal { get; init; }
        public bool IncludePosCharge { get; init; }
        public bool IncludePosChargeMerchant { get; init; }
        public bool IncludeDepositConsumer { get; init; }
        public bool IncludeChainedCharge { get; init; }
        public bool IncludeDepositDepositCapturePerson { get; init; }
        public bool IncludeAcquirerCharge { get; init; }
        public bool IncludeMerchantAcquirerConfiguration { get; init; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}