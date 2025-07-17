export enum Entity {
    Merchants,
    Channels,
    ChannelProfiles,
    PosIntegrations,
    Orders,
    MenuCategories,
    MenuItems,
    Sessions,
    OrderFields,
    Transactions,
    PaymentMethods,
    Invoices,
    Reviews,
    PostCheckoutMessages,
    PostCheckoutLinks,
}

export const getEntityType = (entity: Entity): string => {
    switch(entity){
        case Entity.Merchants: return "Merchants";
        case Entity.Channels: return "Channels";
        case Entity.ChannelProfiles: return "ChannelProfiles";
        case Entity.PosIntegrations: return "PosIntegrations";
        case Entity.Orders: return "Orders";
        case Entity.MenuCategories: return "MenuCategories";
        case Entity.MenuItems: return "MenuItems";
        case Entity.Sessions: return "Sessions";
        case Entity.OrderFields: return "OrderFields";
        case Entity.Transactions: return "Transactions";
        case Entity.PaymentMethods: return "PaymentMethods";
        case Entity.Invoices: return "Invoices";
        case Entity.Reviews: return "Reviews";
        case Entity.PostCheckoutMessages: return "PostCheckoutMessages";
        case Entity.PostCheckoutLinks: return "PostCheckoutLinks";
    }
}