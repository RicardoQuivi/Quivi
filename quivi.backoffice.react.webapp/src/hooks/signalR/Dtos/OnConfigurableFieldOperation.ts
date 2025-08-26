import { Operation } from "./Operation";

export interface OnConfigurableFieldOperation {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}