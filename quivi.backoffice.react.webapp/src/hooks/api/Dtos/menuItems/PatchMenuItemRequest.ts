import { Language } from "../Language";
import { PriceType } from "./PriceType";

export interface PatchMenuItemRequest {
    readonly id: string;
    readonly name?: string;
    readonly description?: string | null;
    readonly imageUrl?: string | null;
    readonly price?: number;
    readonly priceType?: PriceType;
    readonly vatRate?: number;
    readonly locationId?: string | null;
    readonly translations?: Record<Language, PatchMenuItemTranslation | undefined>;
    readonly menuCategoryIds?: string[];
    readonly modifierGroupIds?: string[];
}

export interface PatchMenuItemTranslation {
    readonly name?: string;
    readonly description?: string;
}