export interface ReceiptLine extends BaseReceiptLine {
    readonly subItems?: BaseReceiptLine[];
}

export interface BaseReceiptLine {
    readonly id: string;
    readonly name?: string;
    readonly amount: number;
    readonly quantity?: number;
    readonly isStroke: boolean;
    readonly discount: number;
    readonly info?: string;
}