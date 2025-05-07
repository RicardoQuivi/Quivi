import { Operation } from "./Operation";

export interface OnItemsModifierGroupEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}