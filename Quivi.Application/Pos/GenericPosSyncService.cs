using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Application.Pos
{
    public class GenericPosSyncService : APosSyncService
    {
        public GenericPosSyncService(IEnumerable<IPosSyncStrategy> dataSyncStrategies,
                                IQueryProcessor queryProcessor,
                                ICommandProcessor commandProcessor,
                                IBackgroundJobHandler backgroundJobHandler,
                                IEventService eventService) : base(dataSyncStrategies, queryProcessor, commandProcessor, backgroundJobHandler, eventService)
        {
        }
    }
}
