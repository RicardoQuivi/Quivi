import { Operation } from "./Operation";

export interface OnPrinterMessageEvent {
    readonly operation: Operation;
    readonly merchantId: string;
    readonly printerId: string;
    readonly messageId: string;
}