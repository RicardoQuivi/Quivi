using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Repositories;
using System.Collections.Concurrent;

namespace Quivi.Infrastructure.Repositories
{
    /// <summary>
    /// A proxy class. The purpose of this class is to postpone events to only when actually things are commited/saved,
    /// avoiding publishing events before things are even stored in the database.
    /// </summary>
    public class CoordinatedUnitOfWork : IEventService, IUnitOfWork, IAsyncDisposable
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IEventService eventService;
        private ConcurrentQueue<Func<Task>> publishQueue;

        private bool isTransaction = false;

        public IMerchantsRepository Merchants => unitOfWork.Merchants;
        public IApplicationUsersRepository Users => unitOfWork.Users;
        public IChannelsRepository Channels => unitOfWork.Channels;
        public IChannelProfilesRepository ChannelProfiles => unitOfWork.ChannelProfiles;
        public IPosIntegrationsRepository PosIntegrations => unitOfWork.PosIntegrations;
        public IItemCategoriesRepository ItemCategories => unitOfWork.ItemCategories;
        public IMenuItemsRepository MenuItems => unitOfWork.MenuItems;
        public ILocationsRepository Locations => unitOfWork.Locations;
        public IEmployeesRepository Employees => unitOfWork.Employees;
        public IItemsModifierGroupsRepository MenuItemModifierGroups => unitOfWork.MenuItemModifierGroups;
        public IOrdersRepository Orders => unitOfWork.Orders;
        public IOrderSequencesRepository OrderSequences => unitOfWork.OrderSequences;
        public IOrderMenuItemsRepository OrderMenuItems => unitOfWork.OrderMenuItems;
        public ISessionsRepository Sessions => unitOfWork.Sessions;
        public ICustomChargeMethodsRepository CustomChargeMethods => unitOfWork.CustomChargeMethods;
        public IPosChargesRepository PosCharges => unitOfWork.PosCharges;
        public IPosChargeSyncAttemptsRepository PosChargeSyncAttempts => unitOfWork.PosChargeSyncAttempts;
        public IPreparationGroupsRepository PreparationGroups => unitOfWork.PreparationGroups;
        public IPreparationGroupItemsRepository PreparationGroupItems => unitOfWork.PreparationGroupItems;
        public IPrinterNotificationsContactsRepository PrinterNotificationsContacts => unitOfWork.PrinterNotificationsContacts;
        public IPrinterWorkersRepository PrinterWorkers => unitOfWork.PrinterWorkers;
        public IPrinterNotificationMessagesRepository PrinterNotificationMessages => unitOfWork.PrinterNotificationMessages;
        public IPrinterMessageTargetsRepository PrinterMessageTargets => unitOfWork.PrinterMessageTargets;
        public IOrderConfigurableFieldsRepository OrderConfigurableFields => unitOfWork.OrderConfigurableFields;
        public IMerchantAcquirerConfigurationsRepository MerchantAcquirerConfigurations => unitOfWork.MerchantAcquirerConfigurations;
        public IPeopleRepository People => unitOfWork.People;
        public IChargesRepository Charges => unitOfWork.Charges;
        public IJournalsRepository Journals => unitOfWork.Journals;
        public IReviewsRepository Reviews => unitOfWork.Reviews;
        public IMerchantInvoiceDocumentsRepository MerchantInvoiceDocuments => unitOfWork.MerchantInvoiceDocuments;
        public IPostingsRepository Postings => unitOfWork.Postings;
        public IOrderConfigurableFieldChannelProfileAssociationsRepository OrderConfigurableFieldChannelProfileAssociations => unitOfWork.OrderConfigurableFieldChannelProfileAssociations;
        public IOrderAdditionalInfosRepository OrderAdditionalInfos => unitOfWork.OrderAdditionalInfos;
        public IPosChargeInvoiceItemsRepository PosChargeInvoiceItems => unitOfWork.PosChargeInvoiceItems;
        public IAvailabilityGroupsRepository Availabilities => unitOfWork.Availabilities;
        public IAvailabilityProfileAssociationsRepository AvailabilityProfileAssociations => unitOfWork.AvailabilityProfileAssociations;
        public IAvailabilityMenuItemAssociationsRepository AvailabilityMenuItemAssociations => unitOfWork.AvailabilityMenuItemAssociations;
        public IWeeklyAvailabilitiesRepository WeeklyAvailabilities => unitOfWork.WeeklyAvailabilities;
        public IReportsRepository Reports => unitOfWork.Reports;
        public ISettlementsRepository Settlements => unitOfWork.Settlements;
        public IMerchantServicesRepository MerchantServices => unitOfWork.MerchantServices;

        public CoordinatedUnitOfWork(IUnitOfWork unitOfWork, IEventService eventService)
        {
            this.unitOfWork = unitOfWork;
            this.eventService = eventService;
            this.publishQueue = new ConcurrentQueue<Func<Task>>();
        }

        public Task Publish(IEvent evt)
        {
            publishQueue.Enqueue(() => eventService.Publish(evt));
            return Task.CompletedTask;
        }

        private async Task RunAll()
        {
            while (publishQueue.TryDequeue(out var taskFunc))
                await taskFunc();
        }

        public async Task<ITransaction> StartTransactionAsync()
        {
            if (isTransaction)
                throw new Exception("A Transaction is already going on");

            isTransaction = true;
            var result = new CoordinatedTransaction(this, await unitOfWork.StartTransactionAsync());
            return result;
        }

        public async Task SaveChangesAsync()
        {
            await unitOfWork.SaveChangesAsync();
            if (isTransaction)
                return;

            await RunAll();
        }

        public void Dispose() => _ = DisposeAsync();

        public async ValueTask DisposeAsync()
        {
            if (isTransaction == false)
                await RunAll();

            unitOfWork.Dispose();
            eventService.Dispose();
        }

        public class CoordinatedTransaction : ITransaction
        {
            private readonly CoordinatedUnitOfWork coordinatedUnitOfWork;
            private readonly ITransaction transaction;

            public CoordinatedTransaction(CoordinatedUnitOfWork coordinatedUnitOfWork, ITransaction transaction)
            {
                this.coordinatedUnitOfWork = coordinatedUnitOfWork;
                this.transaction = transaction;
            }
            public async Task CommitAsync()
            {
                await transaction.CommitAsync();
                await coordinatedUnitOfWork.RunAll();
            }

            public async ValueTask DisposeAsync()
            {
                await transaction.DisposeAsync();
                coordinatedUnitOfWork.isTransaction = false;
            }

            public async Task RollbackAsync()
            {
                await transaction.RollbackAsync();
                coordinatedUnitOfWork.publishQueue.Clear();
            }
        }
    }
}
