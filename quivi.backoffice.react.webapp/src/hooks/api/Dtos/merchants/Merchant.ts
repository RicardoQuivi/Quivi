export interface Merchant {
    readonly id: string;
    readonly name: string;
    readonly vatNumber: string;
    readonly logoUrl: string;
    readonly setUpFee?: number;
    readonly transactionFee?: number;
}