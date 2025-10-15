export interface Sales {
    readonly from: string;
    readonly to: string;

    readonly total: number;
    readonly payment: number;
    readonly tip: number;
    readonly totalRefund: number;
    readonly paymentRefund: number;
    readonly tipRefund: number;
}