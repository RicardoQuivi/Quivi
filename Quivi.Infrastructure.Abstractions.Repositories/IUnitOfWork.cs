namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IMerchantsRepository Merchants { get; }
        IApplicationUsersRepository Users { get; }
        IChannelsRepository Channels { get; }
        IChannelProfilesRepository ChannelProfiles { get; }
        IPosIntegrationsRepository PosIntegrations { get; }
        IItemCategoriesRepository ItemCategories { get; }
        IMenuItemsRepository MenuItems { get; }
        ILocationsRepository Locations { get; }
        IEmployeesRepository Employees { get; }
        IItemsModifierGroupsRepository MenuItemModifierGroups { get; }
        IOrdersRepository Orders { get; }
        IOrderMenuItemsRepository OrderMenuItems { get; }
        ISessionsRepository Sessions { get; }
        ICustomChargeMethodsRepository CustomChargeMethods { get; }
        IPosChargesRepository PosCharges { get; }
        IPosChargeSyncAttemptsRepository PosChargeSyncAttempts { get; }
        IPreparationGroupsRepository PreparationGroups { get; }
        IPreparationGroupItemsRepository PreparationGroupItems { get; }
        IPrinterNotificationsContactsRepository PrinterNotificationsContacts { get; }
        IPrinterWorkersRepository PrinterWorkers { get; }
        IPrinterNotificationMessagesRepository PrinterNotificationMessages { get; }
        IPrinterMessageTargetsRepository PrinterMessageTargets { get; }

        Task<ITransaction> StartTransactionAsync();
        Task SaveChangesAsync();
    }
}