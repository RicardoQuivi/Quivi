export interface BaseTransactionItem {
    readonly id: string;
    readonly transactionId: string;
    readonly quantity: number;
    readonly name: string;
    readonly price: number;
    readonly originalPrice: number;
    readonly createdDate: string;
    readonly lastModified: string;
    readonly appliedDiscountPercentage: number;
}


export interface TransactionItem extends BaseTransactionItem {
    readonly modifiers: BaseTransactionItem[];
}