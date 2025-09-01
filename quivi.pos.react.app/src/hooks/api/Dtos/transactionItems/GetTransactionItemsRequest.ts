export interface GetTransactionItemsRequest {
    readonly transactionId: string;

    readonly page: number;
    readonly pageSize?: number;
}