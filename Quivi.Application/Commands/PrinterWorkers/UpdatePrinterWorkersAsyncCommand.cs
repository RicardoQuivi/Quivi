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
    public interface IUpdatablePrinterWorker : IUpdatableEntity
    {
        int Id { get; }
        int MerchantId { get; }
        string? Name { get; set; }
        bool IsDeleted { get; set; }
    }

    public class UpdatePrinterWorkersAsyncCommand : AUpdateAsyncCommand<IEnumerable<PrinterWorker>, IUpdatablePrinterWorker>
    {
        public required GetPrinterWorkersCriteria Criteria { get; init; }
    }

    public class UpdatePrinterWorkersAsyncCommandHandler : ICommandHandler<UpdatePrinterWorkersAsyncCommand, Task<IEnumerable<PrinterWorker>>>
    {
        private readonly IPrinterWorkersRepository repository;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEventService eventService;

        public UpdatePrinterWorkersAsyncCommandHandler(IPrinterWorkersRepository repository,
                                                IDateTimeProvider dateTimeProvider,
                                                IEventService eventService)
        {
            this.repository = repository;
            this.dateTimeProvider = dateTimeProvider;
            this.eventService = eventService;
        }

        public async Task<IEnumerable<PrinterWorker>> Handle(UpdatePrinterWorkersAsyncCommand command)
        {
            var entities = await repository.GetAsync(command.Criteria);
            if (entities.Any() == false)
                return [];

            var now = dateTimeProvider.GetUtcNow();
            List<UpdatablePrinterWorker> changedEntities = new();
            foreach (var entity in entities)
            {
                var updatableEntity = new UpdatablePrinterWorker(entity, now);
                await command.UpdateAction.Invoke(updatableEntity);
                if (updatableEntity.HasChanges)
                {
                    entity.ModifiedDate = now;
                    changedEntities.Add(updatableEntity);
                }
            }

            if (changedEntities.Any() == false)
                return entities;

            await repository.SaveChangesAsync();

            foreach (var entity in changedEntities)
                await eventService.Publish(new OnPrinterWorkerOperationEvent
                {
                    Id = entity.Id,
                    MerchantId = entity.MerchantId,
                    Operation = entity.WasDeleted ? EntityOperation.Delete : EntityOperation.Update,
                });

            return entities;
        }

        private class UpdatablePrinterWorker : IUpdatablePrinterWorker
        {
            public PrinterWorker Model { get; }
            private readonly string? originalName;
            private readonly bool originalIsDeleted;
            private readonly DateTime now;

            public UpdatablePrinterWorker(PrinterWorker model, DateTime now)
            {
                Model = model;
                originalName = model.Name;
                originalIsDeleted = IsDeleted;
                this.now = now;
            }

            public int Id => this.Model.Id;
            public int MerchantId => this.Model.MerchantId;
            public string? Name
            { 
                get => Model.Name;
                set => Model.Name = value;
            }
            public bool IsDeleted 
            {
                get => Model.DeletedDate.HasValue;
                set => Model.DeletedDate = value ? now : null;
            }

            public bool NameChanged => originalName != Model.Name;
            public bool DeletedChange => originalIsDeleted != IsDeleted;
            public bool WasDeleted => !originalIsDeleted && IsDeleted;
            public bool HasChanges => NameChanged || DeletedChange;
        }
    }
}