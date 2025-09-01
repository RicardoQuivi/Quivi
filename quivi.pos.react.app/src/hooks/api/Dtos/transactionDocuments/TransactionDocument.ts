import { TransactionInvoiceType } from "./TransactionInvoiceType";

export interface TransactionDocument {
    readonly id: string;
    readonly name: string;
    readonly url: string;
    readonly type: TransactionInvoiceType;
}