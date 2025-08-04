using Quivi.Application.Pos.SyncStrategies;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Extensions;
using System.Security.Principal;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class PosIntegrationMapperHandler : IMapperHandler<PosIntegration, Dtos.PosIntegration>
    {
        private readonly IIdConverter idConverter;
        private readonly IPosSyncService posSyncService;
        private readonly IPrincipal principal;

        public PosIntegrationMapperHandler(IIdConverter idConverter,
                                            IPosSyncService posSyncService,
                                            IPrincipal principal)
        {
            this.idConverter = idConverter;
            this.posSyncService = posSyncService;
            this.principal = principal;
        }

        public Dtos.PosIntegration Map(PosIntegration model)
        {
            var strategy = posSyncService.Get(model.IntegrationType);
            var settings = strategy?.ParseSyncSettings(model);
            return new Dtos.PosIntegration
            {
                Id = idConverter.ToPublicId(model.Id),
                Type = model.IntegrationType,
                IsActive = !model.DeletedDate.HasValue,
                SyncState = model.DeletedDate.HasValue ? SyncState.Unknown : model.SyncState,
                IsDianosticErrorsMuted = model.DiagnosticErrorsMuted,
                Settings = GetSettings(model, settings),
                Features = new Dtos.IntegrationFeatures
                {
                    AllowsOpeningSessions = settings?.AllowsOpeningSessions ?? false,
                    AllowsEscPosInvoices = settings?.AllowsEscPosInvoices ?? false,
                    AllowsAddingItemsToSession = settings?.AllowsAddingItemsToSession ?? false,
                    AllowsRemovingItemsFromSession = settings?.AllowsRemovingItemsFromSession ?? false,
                    AllowsRefunds = true,
                },
            };
        }

        private IReadOnlyDictionary<IntegrationType, object> GetSettings(PosIntegration model, ISyncSettings? settings)
        {
            var result = new Dictionary<IntegrationType, object>();
            if (principal.IsAdmin() == false)
                return result;

            switch (model.IntegrationType)
            {
                case IntegrationType.QuiviViaFacturalusa:
                    QuiviFacturalusaSyncSettings qvSettings = settings as QuiviFacturalusaSyncSettings ?? new QuiviFacturalusaSyncSettings(model);
                    result.Add(IntegrationType.QuiviViaFacturalusa, new
                    {
                        qvSettings.AccessToken,
                        qvSettings.SkipInvoice,
                        qvSettings.InvoicePrefix,
                        qvSettings.IncludeTipInInvoice,
                    });
                    break;
            }
            return result;
        }
    }
}