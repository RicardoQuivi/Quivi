export interface BaseTransactionItem {
    readonly id: string;
    readonly name: string;
    readonly quantity: number;
    readonly originalPrice: number;
    readonly finalPrice: number;
}

export interface TransactionItem extends BaseTransactionItem {
    readonly modifiers: BaseTransactionItem[];
}