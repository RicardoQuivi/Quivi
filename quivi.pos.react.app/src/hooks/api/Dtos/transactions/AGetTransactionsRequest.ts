export interface AGetTransactionsRequest {
    readonly ids?: string[];
    readonly orderIds: string[];
    readonly search?: string;
    readonly fromDate?: string;
    readonly toDate?: string;
    readonly sessionIds?: string[];
    readonly chargeMethodId?: string;
}