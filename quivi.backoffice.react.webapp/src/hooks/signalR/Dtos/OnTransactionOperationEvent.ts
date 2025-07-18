import { Operation } from "./Operation";

export interface OnTransactionOperationEvent {
    readonly operation: Operation;
    readonly id: string;
    readonly merchantId: string;
    readonly channelId: string;
}