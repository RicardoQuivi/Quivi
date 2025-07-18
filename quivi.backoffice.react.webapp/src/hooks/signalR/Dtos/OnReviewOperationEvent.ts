import { Operation } from "./Operation";

export interface OnReviewOperationEvent {
    readonly operation: Operation;
    readonly id: string;
    readonly merchantId: string;
    readonly channelId: string;
}