import { PagedResponse } from "../PagedResponse";
import { Transaction } from "./Transaction";

export interface GetTransactionsResponse extends PagedResponse<Transaction> {
    
}