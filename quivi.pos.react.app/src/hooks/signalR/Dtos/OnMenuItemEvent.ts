import { Operation } from "./Operation";

export interface OnMenuItemEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}