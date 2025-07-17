using Quivi.Domain.Entities.Charges;

namespace Quivi.Infrastructure.Abstractions.Repositories.Criterias
{
    public record GetChargesCriteria : IPagedCriteria
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<ChargeStatus>? Statuses { get; init; }

        public bool IncludeDeposit { get; set; }
        public bool IncludeDepositDepositCapture { get; set; }
        public bool IncludeDepositCaptureJournal { get; set; }
        public bool IncludeDepositDepositSurcharge { get; set; }
        public bool IncludeDepositSurchargeJournal { get; set; }
        public bool IncludePosCharge { get; set; }
        public bool IncludeDepositConsumer { get; set; }
        public bool IncludeChainedCharge { get; set; }
        public bool IncludeDepositDepositCapturePerson { get; set; }

        public int PageIndex { get; init; }
        public int? PageSize { get; init; }
    }
}