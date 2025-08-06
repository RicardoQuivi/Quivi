export interface RefundTransactionResquest {
    readonly id: string;
    readonly amount?: number;
    readonly cancelation?: boolean;
}