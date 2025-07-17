import type { InvoiceType } from "./InvoiceType";

export interface Invoice {
    readonly id: string;
    readonly transactionId: string;
    readonly name: string;
    readonly type: InvoiceType;
    readonly url: string;
}