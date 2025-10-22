export interface SettlementDetail {
    readonly id: string;
    readonly settlementId: string;
    readonly date: string;
    
    readonly grossAmount: number;
    readonly grossTip: number;
    readonly grossTotal: number;

    readonly netAmount: number;
    readonly netTip: number;
    readonly netTotal: number;
}