import { Operation } from "./Operation";

export interface OnLocalEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}