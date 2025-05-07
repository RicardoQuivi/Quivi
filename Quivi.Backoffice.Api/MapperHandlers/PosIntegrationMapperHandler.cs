using Newtonsoft.Json.Linq;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class PosIntegrationMapperHandler : IMapperHandler<PosIntegration, Dtos.PosIntegration>, IMapperHandler<Dtos.PosIntegration, PosIntegration>
    {
        private readonly IIdConverter idConverter;
        private readonly IPosSyncService posSyncService;

        public PosIntegrationMapperHandler(IIdConverter idConverter,
                                            IPosSyncService posSyncService)
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
                Type = model.IntegrationType,
                ConnectionStringParams = GetConnectionParameters(model),
                IsActive = !model.DeletedDate.HasValue,
                SyncState = model.DeletedDate.HasValue ? SyncState.Unknown : model.SyncState,
                IsDianosticErrorsMuted = model.DiagnosticErrorsMuted,
                Features = new Dtos.IntegrationFeatures
                {
                    AllowsOpeningSessions = settings?.AllowsOpeningSessions ?? false,
                    AllowsEscPosInvoices = settings?.AllowsEscPosInvoices ?? false,
                    AllowsAddingItemsToSession = settings?.AllowsAddingItemsToSession ?? false,
                    AllowsRemovingItemsFromSession = settings?.AllowsRemovingItemsFromSession ?? false,
                },
            };
        }

        private static IReadOnlyDictionary<string, object?> ToDictionary(JObject? jObject)
        {
            Dictionary<string, object?> properties = new Dictionary<string, object?>();
            if(jObject == null)
                return properties;

            foreach (var entry in jObject)
            {
                if (entry.Value is JValue jValue)
                    properties.Add(entry.Key, jValue.Value);
                else if (entry.Value is JObject innerJObject)
                    properties.Add(entry.Key, ToDictionary(innerJObject));
                else
                    throw new NotImplementedException("Implement me");
            }
            return properties;
        }

        private static IReadOnlyDictionary<string, object?> GetConnectionParameters(PosIntegration model)
        {
            var jObject = Newtonsoft.Json.JsonConvert.DeserializeObject(model.ConnectionString) as JObject;
            return ToDictionary(jObject);
        }

        public PosIntegration Map(Dtos.PosIntegration model)
        {
            return new PosIntegration
            {
                Id = string.IsNullOrWhiteSpace(model.Id) ? 0 : idConverter.FromPublicId(model.Id),
                IntegrationType = model.Type,
                ConnectionString = FromConnectionParameters(model.ConnectionStringParams),
                DeletedDate = model.IsActive ? null : DateTime.UtcNow,
                SyncState = model.SyncState,
                DiagnosticErrorsMuted = model.IsDianosticErrorsMuted,
            };
        }

        private static string FromConnectionParameters(object? connectionString)
        {
            if (connectionString == null)
                return "{}";
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(connectionString);
            return result;
        }
    }
}
