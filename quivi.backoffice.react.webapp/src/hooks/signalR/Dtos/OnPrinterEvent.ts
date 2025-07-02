import { Operation } from "./Operation";

export interface OnPrinterEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}