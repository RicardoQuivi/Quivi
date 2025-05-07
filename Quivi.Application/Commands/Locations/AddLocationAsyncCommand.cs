using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.Locations;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.Locations
{
    public class AddLocationAsyncCommand : ICommand<Task<Location?>>
    {
        public int MerchantId { get; init; }
        public required string Name { get; init; }
        public required Action OnInvalidName { get; init; }
        public required Action OnNameAlreadyExists { get; init; }
    }

    public class AddLocationAsyncCommandHandler : ICommandHandler<AddLocationAsyncCommand, Task<Location?>>
    {
        private readonly ILocationsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddLocationAsyncCommandHandler(ILocationsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<Location?> Handle(AddLocationAsyncCommand command)
        {
            if(string.IsNullOrWhiteSpace(command.Name))
            {
                command.OnInvalidName();
                return null;
            }

            var alreadyExistsQuery = await repository.GetAsync(new GetLocationsCriteria
            {
                MerchantIds = [command.MerchantId],
                Names = [command.Name],
                PageSize = 0,
            });
            if(alreadyExistsQuery.TotalItems > 0)
            {
                command.OnNameAlreadyExists();
                return null;
            }

            var now = dateTimeProvider.GetUtcNow();
            var entity = new Location
            {
                MerchantId = command.MerchantId,
                Name = command.Name,
                CreatedDate = now,
                ModifiedDate = now,
                DeletedDate = null,
            };
            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnLocationOperationEvent
            {
                Id = entity.Id,
                MerchantId = entity.MerchantId,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    };
}