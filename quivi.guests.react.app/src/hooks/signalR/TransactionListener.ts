import type { OnTransactionInvoiceOperationEvent } from "./dtos/OnTransactionInvoiceOperationEvent";

export interface TransactionListener {
    readonly transactionId: string;
    readonly onTransactionInvoiceOperation: (evt: OnTransactionInvoiceOperationEvent) => any;
}