interface BaseSessionItem {
    readonly menuItemId: string;
    readonly quantity: number;
    readonly price: number;
    readonly originalPrice: number;
}

export interface SessionExtraItem extends BaseSessionItem  {
    readonly modifierGroupId: string;
}

export interface SessionItem extends BaseSessionItem {
    readonly id: string;
    readonly extras: SessionExtraItem[];
    readonly isPaid: boolean;
    readonly discountPercentage: number;
    readonly lastModified: string;
}