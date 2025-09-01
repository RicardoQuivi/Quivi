export interface GetTransactionDocumentsRequest {
    readonly transactionId: string;

    readonly page: number;
    readonly pageSize?: number;
}