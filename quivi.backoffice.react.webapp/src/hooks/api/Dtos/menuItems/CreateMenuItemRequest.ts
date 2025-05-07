import { Language } from "../Language";
import { PriceType } from "./PriceType";

export interface CreateMenuItemRequest {
    readonly name: string;
    readonly description?: string;
    readonly imageUrl?: string;
    readonly price: number;
    readonly priceType: PriceType;
    readonly vatRate: number;
    readonly locationId?: string;
    readonly translations?: Record<Language, CreateMenuItemTranslation>;
    readonly menuCategoryIds?: string[];
}

interface CreateMenuItemTranslation {
    readonly name: string;
    readonly description?: string;
}