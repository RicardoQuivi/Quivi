import type { ChargeMethod } from "../ChargeMethod";
import type { SyncStatus } from "./SyncStatus";
import type { TransactionStatus } from "./TransactionStatus";

export interface TopUpData {
    readonly continuationId: string;
}

export interface RefundData {
    readonly refund: number;
}

export interface PaybyrdMbWayAdditionalData {
    readonly CheckoutKey: string;
    readonly OrderId: string;
    readonly Culture: string;
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
    readonly additionalData?: PaybyrdMbWayAdditionalData | any;
}

export const asPaybyrdMbWayAdditionalData = (data: any): PaybyrdMbWayAdditionalData | undefined => {
    if(data !== null && typeof data === 'object' &&
           'CheckoutKey' in data &&  typeof data['CheckoutKey'] === 'string' &&
           'OrderId' in data &&  typeof data['OrderId'] === 'string' &&
           'Culture' in data &&  typeof data['Culture'] === 'string') {
            return data as PaybyrdMbWayAdditionalData;
    }
    return undefined;
}