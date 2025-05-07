import { Operation } from "./Operation";

export interface OnEmployeeEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}