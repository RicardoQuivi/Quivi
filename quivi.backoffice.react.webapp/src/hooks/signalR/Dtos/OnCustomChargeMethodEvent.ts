import { Operation } from "./Operation";

export interface OnCustomChargeMethodEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}