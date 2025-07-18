import { Operation } from "./Operation";

export interface OnTransactionSyncAttemptOperationEvent {
    readonly operation: Operation;
    readonly posChargeId: string;
    readonly merchantId: string;
    readonly channelId: string;
}