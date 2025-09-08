export interface RefundTransactionRequest {
    readonly id: string;
    readonly amount: number;
    readonly ignoreAcquireRefundErrors: boolean;
    readonly isCancellation?: boolean;
    readonly refundReason?: string;
}