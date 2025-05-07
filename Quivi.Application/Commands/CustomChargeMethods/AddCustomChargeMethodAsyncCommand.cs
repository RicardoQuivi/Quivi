using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.CustomChargeMethods;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.CustomChargeMethods
{
    public class AddCustomChargeMethodAsyncCommand : ICommand<Task<CustomChargeMethod?>>
    {
        public int MerchantId { get; init; }
        public required string Name { get; init; }
        public string? LogoUrl { get; init; }
        public required Action OnInvalidName { get; init; }
        public required Action OnNameAlreadyExists { get; init; }
    }

    public class AddCustomChargeMethodAsyncCommandHandler : ICommandHandler<AddCustomChargeMethodAsyncCommand, Task<CustomChargeMethod?>>
    {
        private readonly ICustomChargeMethodsRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public AddCustomChargeMethodAsyncCommandHandler(ICustomChargeMethodsRepository repository, IDateTimeProvider dateTimeProvider, IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<CustomChargeMethod?> Handle(AddCustomChargeMethodAsyncCommand command)
        {
            if(string.IsNullOrWhiteSpace(command.Name))
            {
                command.OnInvalidName();
                return null;
            }

            var alreadyExistsQuery = await repository.GetAsync(new GetCustomChargeMethodsCriteria
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
            var entity = new CustomChargeMethod
            {
                MerchantId = command.MerchantId,
                Name = command.Name,
                Logo = command.LogoUrl,
                CreatedDate = now,
                ModifiedDate = now,
            };
            repository.Add(entity);
            await repository.SaveChangesAsync();

            await eventService.Publish(new OnCustomChargeMethodOperationEvent
            {
                Id = entity.Id,
                MerchantId = entity.MerchantId,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    };
}