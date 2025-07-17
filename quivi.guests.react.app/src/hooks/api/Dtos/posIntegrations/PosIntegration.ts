import type { PosIntegrationState } from "./PosIntegrationState";

export interface PosIntegration {
    readonly id: string;
    readonly allowsInvoiceDownloads: boolean;
    readonly allowsEscPosInvoices: boolean;
    readonly allowsOpeningSessions: boolean;
    readonly allowsRemovingItemsFromSession: boolean;
    readonly allowsAddingItemsToSession: boolean;
    readonly allowsMenuSyncing: boolean;
    readonly allowsPayments: boolean;
    readonly state: PosIntegrationState;
    readonly isActive: boolean; 
}