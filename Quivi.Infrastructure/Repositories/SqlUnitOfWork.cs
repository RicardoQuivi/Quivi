using Microsoft.EntityFrameworkCore.Storage;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlUnitOfWork : IUnitOfWork
    {
        private readonly QuiviContext quiviContext;

        public SqlUnitOfWork(QuiviContext quiviContext)
        {
            this.quiviContext = quiviContext;
        }

        public IMerchantsRepository Merchants => new SqlMerchantsRepository(quiviContext);
        public IApplicationUsersRepository Users => new SqlApplicationUsersRepository(quiviContext);
        public IChannelsRepository Channels => new SqlChannelsRepository(quiviContext);
        public IChannelProfilesRepository ChannelProfiles => new SqlChannelProfilesRepository(quiviContext);
        public IPosIntegrationsRepository PosIntegrations => new SqlPosIntegrationsRepository(quiviContext);
        public IItemCategoriesRepository ItemCategories => new SqlItemCategoriesRepository(quiviContext);
        public IMenuItemsRepository MenuItems => new SqlMenuItemsRepository(quiviContext);
        public ILocationsRepository Locations => new SqlLocationsRepository(quiviContext);
        public IEmployeesRepository Employees => new SqlEmployeesRepository(quiviContext);
        public IItemsModifierGroupsRepository MenuItemModifierGroups => new SqlItemsModifierGroupsRepository(quiviContext);
        public IOrdersRepository Orders => new SqlOrdersRepository(quiviContext);
        public IOrderMenuItemsRepository OrderMenuItems => new SqlOrderMenuItemsRepository(quiviContext);
        public ISessionsRepository Sessions => new SqlSessionsRepository(quiviContext);
        public ICustomChargeMethodsRepository CustomChargeMethods => new SqlCustomChargeMethodsRepository(quiviContext);
        public IPosChargesRepository PosCharges => new SqlPosChargesRepository(quiviContext);
        public IPosChargeSyncAttemptsRepository PosChargeSyncAttempts => new SqlPosChargeSyncAttemptsRepository(quiviContext);
        public IPreparationGroupsRepository PreparationGroups => new SqlPreparationGroupsRepository(quiviContext);
        public IPreparationGroupItemsRepository PreparationGroupItems => new SqlPreparationGroupItemsRepository(quiviContext);
        public IPrinterNotificationsContactsRepository PrinterNotificationsContacts => new SqlPrinterNotificationsContactRepository(quiviContext);
        public IPrinterWorkersRepository PrinterWorkers => new SqlPrinterWorkersRepository(quiviContext);

        public Task SaveChangesAsync() => quiviContext.SaveChangesAsync();
        public void Dispose() => quiviContext.Dispose();

        public async Task<ITransaction> StartTransactionAsync()
        {
            var transaction = await quiviContext.Database.BeginTransactionAsync();
            return new Transaction(transaction);
        }

        private class Transaction : ITransaction
        {
            private readonly IDbContextTransaction _dbContextTransaction;

            public Transaction(IDbContextTransaction dbContextTransaction)
            {
                _dbContextTransaction = dbContextTransaction;
            }

            public Task CommitAsync() => _dbContextTransaction.CommitAsync();

            public ValueTask DisposeAsync() => _dbContextTransaction.DisposeAsync();

            public Task RollbackAsync() => _dbContextTransaction.RollbackAsync();
        }
    }
}
