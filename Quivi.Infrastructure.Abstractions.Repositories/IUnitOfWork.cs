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
        IOrderSequencesRepository OrderSequences { get; }
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
        IOrderConfigurableFieldsRepository OrderConfigurableFields { get; }
        IMerchantAcquirerConfigurationsRepository MerchantAcquirerConfigurations { get; }
        IPeopleRepository People { get; }
        IChargesRepository Charges { get; }
        IJournalsRepository Journals { get; }
        IReviewsRepository Reviews { get; }
        IMerchantInvoiceDocumentsRepository MerchantInvoiceDocuments { get; }
        IPostingsRepository Postings { get; }
        IOrderConfigurableFieldChannelProfileAssociationsRepository OrderConfigurableFieldChannelProfileAssociations { get; }
        IOrderAdditionalInfosRepository OrderAdditionalInfos { get; }
        IPosChargeInvoiceItemsRepository PosChargeInvoiceItems { get; }
        IAvailabilityGroupsRepository Availabilities { get; }
        IAvailabilityProfileAssociationsRepository AvailabilityProfileAssociations { get; }
        IAvailabilityMenuItemAssociationsRepository AvailabilityMenuItemAssociations { get; }
        IWeeklyAvailabilitiesRepository WeeklyAvailabilities { get; }

        Task<ITransaction> StartTransactionAsync();
        Task SaveChangesAsync();
    }
}