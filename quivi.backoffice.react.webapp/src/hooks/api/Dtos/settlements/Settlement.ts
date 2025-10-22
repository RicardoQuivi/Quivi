export interface Settlement {
    readonly id: string;
    readonly date: string;
    
    readonly grossAmount: number;
    readonly grossTip: number;
    readonly grossTotal: number;

    readonly netAmount: number;
    readonly netTip: number;
    readonly netTotal: number;
}