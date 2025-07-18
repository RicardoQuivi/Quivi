import { PagedRequest } from "../PagedRequest";
import { ATransactionsRequest } from "./ATransactionsRequest";

export interface GetTransactionsRequest extends PagedRequest, ATransactionsRequest {
    readonly ids?: string[];
}