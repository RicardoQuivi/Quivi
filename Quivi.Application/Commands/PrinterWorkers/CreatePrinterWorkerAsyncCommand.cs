using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data;
using Quivi.Infrastructure.Abstractions.Events.Data.PrinterWorkers;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Application.Commands.PrinterWorkers
{
    public class CreatePrinterWorkerAsyncCommand : ICommand<Task<PrinterWorker?>>
    {
        public int MerchantId { get; init; }
        public required string Identifier { get; init; }
        public required string Name { get; init; }
        public required Action OnInvalidIdentifier { get; init; }
        public required Action OnDuplicateIdentifier { get; init; }
    }

    public class CreatePrinterWorkerAsyncCommandHandler : ICommandHandler<CreatePrinterWorkerAsyncCommand, Task<PrinterWorker?>>
    {
        private readonly IPrinterWorkersRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public CreatePrinterWorkerAsyncCommandHandler(IPrinterWorkersRepository repository,
                                                        IDateTimeProvider dateTimeProvider,
                                                        IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<PrinterWorker?> Handle(CreatePrinterWorkerAsyncCommand command)
        {
            if(string.IsNullOrWhiteSpace(command.Identifier))
            {
                command.OnInvalidIdentifier();
                return null;
            }

            var existentRecordQuery = await repository.GetAsync(new GetPrinterWorkersCriteria
            {
                Identifiers = [command.Identifier],
                PageSize = 1,
            });

            var now = dateTimeProvider.GetUtcNow();
            var entity = existentRecordQuery.FirstOrDefault();
            if(entity != null)
            {
                if (entity.DeletedDate.HasValue == false)
                {
                    command.OnDuplicateIdentifier();
                    return null;
                }

                entity.ModifiedDate = now;
                entity.DeletedDate = null;
                entity.MerchantId = command.MerchantId;
            }
            else
            {
                entity = new PrinterWorker
                {
                    Identifier = command.Identifier,
                    MerchantId = command.MerchantId,
                    Name = command.Name,
                    CreatedDate = now,
                    ModifiedDate = now,
                };
                repository.Add(entity);
            }

            await repository.SaveChangesAsync();

            await eventService.Publish(new OnPrinterWorkerOperationEvent
            {
                Id = entity.Id,
                MerchantId = command.MerchantId,
                Operation = EntityOperation.Create,
            });
            return entity;
        }
    }
}