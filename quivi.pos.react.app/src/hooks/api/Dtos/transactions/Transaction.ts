export interface Transaction {
    readonly id: string;
    readonly isSynced: boolean;
    readonly channelId: string;
    readonly employeeId?: string;
    readonly customChargeMethodId?: string;
    readonly payment: number;
    readonly tip: number;
    readonly refundedAmount: number;
    readonly isFreePayment: boolean;
    readonly sessionId?: string;
    readonly email?: string;
    readonly vatNumber?: string;
    readonly refundEmployeeId?: string;
    readonly capturedDate: string;
    readonly lastModified: string;
}