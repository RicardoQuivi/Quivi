import { PagedRequest } from "../PagedRequest";
import { AGetTransactionsRequest } from "./AGetTransactionsRequest";

export interface GetTransactionsRequest extends PagedRequest, AGetTransactionsRequest {
}