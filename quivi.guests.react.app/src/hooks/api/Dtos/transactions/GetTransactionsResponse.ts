import type { PagedResponse } from "../PagedResponse";
import type { Transaction } from "./Transaction";

export interface GetTransactionsResponse extends PagedResponse<Transaction> {
}