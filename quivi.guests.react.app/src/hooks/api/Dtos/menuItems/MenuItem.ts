export interface MenuItemModifierGroup {
    readonly id: string;
    readonly name: string;
    readonly minSelection: number;
    readonly maxSelection: number;

    readonly options: BaseMenuItem[];
}

export interface MenuItem extends BaseMenuItem {
    readonly modifiers: MenuItemModifierGroup[];
}

export interface BaseMenuItem {
    readonly id: string;
    readonly name: string;
    readonly description?: string;
    readonly imageUrl?: string;
    readonly price: number;
    readonly priceType: string;
    readonly isAvailable: boolean;
}