import { Operation } from "./Operation";

export interface OnPrinterWorkerEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly id: string;
}