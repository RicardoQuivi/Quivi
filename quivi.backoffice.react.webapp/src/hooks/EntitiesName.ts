export enum Entity {
    Merchants,
    Channels,
    ChannelProfiles,
    PosIntegrations,
    MenuCategories,
    MenuItems,
    Locals,
    Employees,
    ModifierGroups,
    CustomChargeMethods,
    PrinterWorkers,
    Printers,
    PrinterMessages,
    AcquirerConfigurations,
    Transactions,
    Reviews,
    MerchantDocuments,
    ConfigurableFields,
    ConfigurableFieldAssociations,
}

export const getEntityType = (entity: Entity): string => {
    switch(entity){
        case Entity.Merchants: return "Merchants";
        case Entity.Channels: return "Channels";
        case Entity.ChannelProfiles: return "ChannelProfiles";
        case Entity.PosIntegrations: return "PosIntegrations";
        case Entity.MenuCategories: return "MenuCategories";
        case Entity.MenuItems: return "MenuItems";
        case Entity.Locals: return "Locals";
        case Entity.Employees: return "Employees";
        case Entity.ModifierGroups: return "ModifierGroups";
        case Entity.CustomChargeMethods: return "CustomChargeMethods";
        case Entity.PrinterWorkers: return "PrinterWorkers";
        case Entity.Printers: return "Printers";
        case Entity.PrinterMessages: return "PrinterMessages";
        case Entity.AcquirerConfigurations: return "AcquirerConfigurations";
        case Entity.Transactions: return "Transactions";
        case Entity.Reviews: return "Reviews";
        case Entity.MerchantDocuments: return "MerchantDocuments";
        case Entity.ConfigurableFields: return "ConfigurableFields";
        case Entity.ConfigurableFieldAssociations: return "ConfigurableFieldAssociations";
    }
}