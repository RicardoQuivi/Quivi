using Quivi.Domain.Entities.Pos;

namespace Quivi.Backoffice.Api.Dtos
{
    public class IntegrationFeatures
    {
        public bool AllowsEscPosInvoices { get; init; }
        public bool AllowsOpeningSessions { get; init; }
        public bool AllowsAddingItemsToSession { get; init; }
        public bool AllowsRemovingItemsFromSession { get; init; }
    }

    public class PosIntegration
    {
        public required string Id { get; init; }
        public IntegrationType Type { get; init; }
        public bool IsActive { get; init; }
        public SyncState SyncState { get; init; }
        public bool IsDianosticErrorsMuted { get; init; }
        public required IReadOnlyDictionary<IntegrationType, object> Settings { get; init; }
        public required IntegrationFeatures Features { get; init; }
    }
}