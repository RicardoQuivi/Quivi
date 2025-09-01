import { PagedResponse } from "../PagedResponse";
import { TransactionDocument } from "./TransactionDocument";

export interface GetTransactionDocumentsResponse extends PagedResponse<TransactionDocument> {
    
}