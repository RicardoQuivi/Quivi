import type { PagedRequest } from "../PagedRequest";

export interface GetInvoicesRequest extends PagedRequest {
    readonly transactionId: string;
}