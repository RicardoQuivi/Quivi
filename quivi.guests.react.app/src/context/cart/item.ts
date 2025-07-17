export interface IItemModifierGroup {
    readonly id: string;
    readonly name: string;
    readonly minSelection: number;
    readonly maxSelection: number;

    readonly options: IBaseItem[];
}

export interface IItem extends IBaseItem {
    readonly modifiers: IItemModifierGroup[];
}

export interface IBaseItem {
    readonly id: string;
    readonly name: string;
    readonly description?: string;
    readonly imageUrl?: string;
    readonly price: number;
    readonly priceType: string;
    readonly isAvailable: boolean;
}