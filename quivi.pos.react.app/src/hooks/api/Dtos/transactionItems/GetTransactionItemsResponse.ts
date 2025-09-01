import { PagedResponse } from "../PagedResponse";
import { TransactionItem } from "./TransactionItem";

export interface GetTransactionItemsResponse extends PagedResponse<TransactionItem> {
    
}