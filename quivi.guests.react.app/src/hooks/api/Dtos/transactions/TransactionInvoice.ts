import type { InvoiceType } from "./InvoiceType";

export interface TransactionInvoice {
    readonly id: string;
    readonly transactionId: string;
    readonly name: string;
    readonly type: InvoiceType;
    readonly downloadUrl: string;
}