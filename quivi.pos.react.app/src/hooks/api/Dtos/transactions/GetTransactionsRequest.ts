import { PagedRequest } from "../PagedRequest";

export interface GetTransactionsRequest extends PagedRequest {
    readonly ids?: string[];
}