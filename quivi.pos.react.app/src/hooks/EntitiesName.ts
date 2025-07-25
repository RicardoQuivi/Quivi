export enum Entity {
    Channels,
    ChannelProfiles,
    PosIntegrations,
    MenuCategories,
    MenuItems,
    Locals,
    Employees,
    ModifierGroups,
    CustomChargeMethods,
    Notifications,
    Sessions,
    Orders,
    PreparationGroups,
    ConfigurableFields,
    Printers,
    BackgroundJobs,
    Transactions,
}

export const getEntityType = (entity: Entity): string => {
    switch(entity){
        case Entity.Channels: return "Channels";
        case Entity.ChannelProfiles: return "ChannelProfiles";
        case Entity.PosIntegrations: return "PosIntegrations";
        case Entity.MenuCategories: return "MenuCategories";
        case Entity.MenuItems: return "MenuItems";
        case Entity.Locals: return "Locals";
        case Entity.Employees: return "Employees";
        case Entity.ModifierGroups: return "ModifierGroups";
        case Entity.CustomChargeMethods: return "CustomChargeMethods";
        case Entity.Notifications: return "Notifications";
        case Entity.Sessions: return "Sessions";
        case Entity.Orders: return "Orders";
        case Entity.PreparationGroups: return "PreparationGroups";
        case Entity.ConfigurableFields: return "ConfigurableFields";
        case Entity.Printers: return "Printers";
        case Entity.BackgroundJobs: return "BackgroundJobs";
        case Entity.Transactions: return "Transactions";
    }
}