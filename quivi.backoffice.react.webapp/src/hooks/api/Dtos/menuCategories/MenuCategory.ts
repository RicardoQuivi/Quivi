import { Language } from "../Language";

export interface MenuCategory {
    readonly id: string;
    readonly name: string;
    readonly imageUrl?: string;
    readonly sortIndex: number;
    readonly translations: Record<Language, MenuCategoryTranslation>;
}

export interface MenuCategoryTranslation {
    readonly name: string;
}