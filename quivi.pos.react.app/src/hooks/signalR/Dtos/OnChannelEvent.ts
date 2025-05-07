import { Operation } from "./Operation";

export interface OnChannelEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}