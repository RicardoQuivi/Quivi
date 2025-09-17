import type { OnReviewOperationEvent } from "./dtos/OnReviewOperationEvent";
import type { OnTransactionInvoiceOperationEvent } from "./dtos/OnTransactionInvoiceOperationEvent";

export interface TransactionListener {
    readonly transactionId: string;
    readonly onTransactionInvoiceOperation?: (evt: OnTransactionInvoiceOperationEvent) => any;
    readonly onReviewOperation?: (evt: OnReviewOperationEvent) => any;
}