import { Operation } from "./Operation";

export interface OnMerchantDocumentOperation {
    readonly operation: Operation;
    readonly id: string;
    readonly merchantId: string;
}