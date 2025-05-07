import { Language } from "../Language";
import { PriceType } from "./PriceType";

export interface MenuItem {
    readonly id: string;
    readonly name: string;
    readonly description?: string;
    readonly price: number;
    readonly priceType: PriceType;
    readonly locationId?: string;
    readonly showWhenNotAvailable: boolean;
    readonly sortIndex: number;
    readonly stock: boolean;
    readonly vatRate: number;
    readonly imageUrl?: string;
    readonly translations: Record<Language, MenuItemTranslation>;
    readonly menuCategoryIds: string[];
}

export interface MenuItemTranslation {
    readonly name: string;
    readonly description?: string;
}