import type { DataResponse } from "../DataResponse";
import type { Transaction } from "./Transaction";

export interface ProcessTransactionResponse extends DataResponse<Transaction> {
    readonly threeDsUrl?: string;
}