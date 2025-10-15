export interface ChargeMethodSales {
    readonly customChargeMethodId?: string;

    readonly from: string;
    readonly to: string;

    readonly totalInvoices: number;
    readonly totalBilledAmount: number;
}