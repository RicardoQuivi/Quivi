import type { ChargeMethod } from "../ChargeMethod";
import type { SyncStatus } from "./SyncStatus";
import type { TransactionStatus } from "./TransactionStatus";

export interface TopUpData {
    readonly continuationId: string;
}

export interface RefundData {
    readonly refund: number;
}

export interface Transaction {
    readonly id: string;
    readonly total: number;
    readonly payment: number;
    readonly tip: number;
    readonly surcharge: number;
    readonly syncedAmount: number;
    readonly capturedDate?: string;
    readonly method: ChargeMethod;
    readonly status: TransactionStatus;
    readonly syncStatus: SyncStatus;
    readonly topUpData?: TopUpData;
    readonly paymentAdditionalData: undefined;
    readonly refundData?: RefundData;
    readonly lastModified: string;
}