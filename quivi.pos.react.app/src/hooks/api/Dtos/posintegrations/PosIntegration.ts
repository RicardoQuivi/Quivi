export interface PosIntegration {
    readonly id: string;
    readonly isOnline: boolean;
    readonly allowsPayments: boolean;
    readonly allowsOpeningSessions: boolean;
    readonly allowsEscPosInvoices: boolean;
    readonly allowsAddingItemsToSession: boolean;
    readonly allowsRemovingItemsFromSession: boolean;
    readonly allowsRefunds: boolean;
}