using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PreparationGroups
{
    public class PrintPreparationGroupAsyncCommand : ICommand<Task<PreparationGroup?>>
    {
        public int MerchantId { get; init; }
        public int PreparationGroupId { get; init; }
        public int? LocationId { get; init; }
    }

    public class PrintPreparationGroupAsyncCommandHandler : ICommandHandler<PrintPreparationGroupAsyncCommand, Task<PreparationGroup?>>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IPreparationGroupsRepository repository;
        private readonly IMapper mapper;
        private readonly IEventService eventService;

        public PrintPreparationGroupAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                        IPreparationGroupsRepository repository,
                                                        IMapper mapper,
                                                        IIdConverter idConverter,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IEventService eventService)
        {
            this.queryProcessor = queryProcessor;
            this.idConverter = idConverter;
            this.dateTimeProvider = dateTimeProvider;
            this.repository = repository;
            this.mapper = mapper;
            this.eventService = eventService;
        }

        public async Task<PreparationGroup?> Handle(PrintPreparationGroupAsyncCommand command)
        {
            var groupsQuery = await repository.GetAsync(new GetPreparationGroupsCriteria
            {
                MerchantIds = [command.MerchantId],
                Ids = [command.PreparationGroupId],
                States = [PreparationGroupState.Committed],
                IncludePreparationGroupItemMenuItems = true,
                IncludeSessionChannel = true,
                IncludeOrders = true,
                IncludeOrderFields = true,
                PageIndex = 0,
                PageSize = 1,
            });

            var group = groupsQuery.SingleOrDefault();
            if (group == null)
                return null;

            //TODO: Implement printing
            return group;
        }
    }
}
