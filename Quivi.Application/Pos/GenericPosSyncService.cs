using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Pos.EscPos;
using Quivi.Infrastructure.Abstractions.Services;
using Quivi.Infrastructure.Abstractions.Storage;

namespace Quivi.Application.Pos
{
    public class GenericPosSyncService : APosSyncService
    {
        public GenericPosSyncService(IEnumerable<IPosSyncStrategy> dataSyncStrategies,
                                IQueryProcessor queryProcessor,
                                ICommandProcessor commandProcessor,
                                IBackgroundJobHandler backgroundJobHandler,
                                IEventService eventService,
                                IStorageService storageService,
                                IIdConverter idConverter,
                                IEscPosPrinterService escPosPrinterService,
                                ILogger logger) : base(dataSyncStrategies, queryProcessor, commandProcessor, backgroundJobHandler, eventService, storageService, idConverter, escPosPrinterService, logger)
        {
        }
    }
}