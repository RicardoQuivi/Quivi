import { PagedRequest } from "../PagedRequest";

export interface GetMerchantDocumentsRequest extends PagedRequest {
    readonly transactionIds?: string[];
    readonly monthlyInvoiceOnly?: boolean;
}