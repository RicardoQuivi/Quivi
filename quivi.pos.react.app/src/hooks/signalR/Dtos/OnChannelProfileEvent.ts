import { Operation } from "./Operation";

export interface OnChannelProfileEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}