using Quivi.Domain.Entities.Financing;

namespace Quivi.Application.Commands.Settlements
{
    public interface IUpdatableSettlement : IUpdatableEntity
    {
        int Id { get; }
        SettlementState State { get; set; }
        DateOnly Date { get; }

        IUpdatableRelationship<IUpdatableSettlementDetail, int> SettlementDetails { get; }
        IUpdatableRelationship<IUpdatableSettlementServiceDetail, int> SettlementServiceDetails { get; }
    }
}