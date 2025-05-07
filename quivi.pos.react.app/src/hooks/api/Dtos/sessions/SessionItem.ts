export interface BaseSessionItem {
    readonly menuItemId: string;
    readonly quantity: number;
    readonly price: number;
    readonly originalPrice: number;
}

export interface SessionItem extends BaseSessionItem {
    readonly id: string;
    readonly extras: BaseSessionItem[];
    readonly isPaid: boolean;
    readonly discountPercentage: number;
    readonly lastModified: string;
}