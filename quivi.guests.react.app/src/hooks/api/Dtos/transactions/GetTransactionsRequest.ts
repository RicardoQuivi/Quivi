import type { PagedRequest } from "../PagedRequest";

export interface GetTransactionsRequest extends PagedRequest {
    readonly id?: string;
    readonly sessionId?: string;
    readonly orderId?: string;
}