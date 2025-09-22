using Quivi.Application.Pos.SyncStrategies;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class PosIntegrationMapperHandler : IMapperHandler<PosIntegration, Dtos.PosIntegration>
    {
        private readonly IIdConverter idConverter;
        private readonly IPosSyncService posSyncService;

        public PosIntegrationMapperHandler(IPosSyncService dataSyncService, IIdConverter idConverter)
        {
            this.posSyncService = dataSyncService;
            this.idConverter = idConverter;
        }

        public Dtos.PosIntegration Map(PosIntegration model)
        {
            var strategy = posSyncService.Get(model.IntegrationType);
            var settings = strategy?.ParseSyncSettings(model) ?? new NoIntegrationSyncSettings();

            return new Dtos.PosIntegration
            {
                Id = idConverter.ToPublicId(model.Id),
                IsOnline = model.SyncState == SyncState.Running,
                AllowsPayments = settings.AllowsPayments,
                AllowsEscPosInvoices = settings.AllowsEscPosInvoices,
                AllowsOpeningSessions = settings.AllowsOpeningSessions,
                AllowsAddingItemsToSession = settings.AllowsAddingItemsToSession,
                AllowsRemovingItemsFromSession = settings.AllowsRemovingItemsFromSession,
                AllowsRefunds = true,
            };
        }
    }
}