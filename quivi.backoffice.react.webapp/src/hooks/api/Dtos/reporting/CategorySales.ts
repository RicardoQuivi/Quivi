export interface CategorySales {
    readonly menuCategoryId: string;

    readonly from: string;
    readonly to: string;

    readonly totalItemsSoldQuantity: number;
    readonly totalBilledAmount: number;
}