import { ChargeMethod } from "../ChargeMethod";
import { TransactionItem } from "./TransactionItem";

export enum SynchronizationState {
    Failed = -1,
    Syncing = 0,
    Succeeded = 1,
}

export enum InvoiceRefundType {
    Unknown = 0,
    CreditNote = 1,
    Cancellation = 2,
}

export interface Transaction {
    readonly id: string;
    readonly chargeMethod: ChargeMethod;
    readonly capturedDate: string;
    readonly payment: number;
    readonly paymentDiscount?: number;
    readonly tip: number;
    readonly surcharge: number;
    readonly refundedAmount: number;
    readonly invoiceRefundType?: InvoiceRefundType;
    readonly email?: string;
    readonly vatNumber?: string;
    readonly syncingState: SynchronizationState;
    readonly sessionId?: string;
    readonly channelId: string;
    readonly customChargeMethodId?: string;
    readonly merchantId: string;
    readonly items: TransactionItem[];
}