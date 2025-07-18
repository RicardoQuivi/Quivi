export interface TransactionItem {
    readonly id: string;
    readonly name: string;
    readonly quantity: number;
    readonly originalPrice: number;
    readonly finalPrice: number;
}