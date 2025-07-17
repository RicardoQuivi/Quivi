using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class PosIntegrationMapperHandler : IMapperHandler<PosIntegration, Dtos.PosIntegration>
    {
        private readonly IIdConverter idConverter;
        private readonly IPosSyncService posSyncService;

        public PosIntegrationMapperHandler(IIdConverter idConverter, IPosSyncService posSyncService)
        {
            this.idConverter = idConverter;
            this.posSyncService = posSyncService;
        }

        public Dtos.PosIntegration Map(PosIntegration model)
        {
            var strategy = posSyncService.Get(model.IntegrationType);
            var settings = strategy?.ParseSyncSettings(model);

            return new Dtos.PosIntegration
            {
                Id = idConverter.ToPublicId(model.Id),
                AllowsAddingItemsToSession = settings?.AllowsAddingItemsToSession ?? false,
                AllowsEscPosInvoices = settings?.AllowsEscPosInvoices ?? false,
                AllowsInvoiceDownloads = settings?.AllowsInvoiceDownloads ?? false,
                AllowsMenuSyncing = settings?.AllowsMenuSyncing ?? false,
                AllowsOpeningSessions = settings?.AllowsOpeningSessions ?? false,
                AllowsPayments = settings?.AllowsPayments ?? false,
                AllowsRemovingItemsFromSession = settings?.AllowsRemovingItemsFromSession ?? false,
                IsActive = model.DeletedDate.HasValue == false,
                State = model.SyncState == SyncState.Running ? Dtos.PosIntegrationState.Running : (model.SyncState == SyncState.PoSOffline ? Dtos.PosIntegrationState.Offline : Dtos.PosIntegrationState.Error),
            };
        }
    }
}