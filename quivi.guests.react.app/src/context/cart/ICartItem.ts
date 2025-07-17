import type { IBaseItem, IItemModifierGroup } from "./item";

export interface ICartItem extends IBaseItem {
    readonly quantity: number;
    readonly modifiers: ICartModifier[];
}

export interface ICartModifier extends IItemModifierGroup {
    readonly selectedOptions: ICartItem[];
}