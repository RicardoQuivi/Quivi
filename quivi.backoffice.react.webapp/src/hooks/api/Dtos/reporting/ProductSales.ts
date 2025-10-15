export interface ProductSales {
    readonly menuItemId: string;

    readonly from: string;
    readonly to: string;

    readonly totalSoldQuantity: number;
    readonly totalBilledAmount: number;
}