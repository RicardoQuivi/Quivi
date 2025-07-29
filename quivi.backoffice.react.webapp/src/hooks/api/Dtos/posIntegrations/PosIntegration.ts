export interface IntegrationFeatures {
    readonly allowsEscPosInvoices: boolean;
    readonly allowsOpeningSessions: boolean;
    readonly allowsAddingItemsToSession: boolean;
    readonly allowsRemovingItemsFromSession: boolean;
}

export enum IntegrationType {
    QuiviViaFacturalusa = 0,
}

export enum PosSyncState {
    SyncFailure = -3,
    PoSOffline = -2,
    Stopped = -1,
    Unknown = 0,
    Running = 1
}

export interface PosIntegration {
    readonly id: string;
    readonly type: IntegrationType;
    readonly isActive: boolean;
    readonly isDianosticErrorsMuted: boolean;
    readonly syncState: PosSyncState;
    readonly features: IntegrationFeatures;
    readonly settings: Record<string, any>;
}

