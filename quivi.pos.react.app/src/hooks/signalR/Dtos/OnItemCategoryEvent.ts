import { Operation } from "./Operation";

export interface OnItemCategoryEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}