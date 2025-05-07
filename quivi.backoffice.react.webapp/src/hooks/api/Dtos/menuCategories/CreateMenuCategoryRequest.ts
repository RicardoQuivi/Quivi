import { Language } from "../Language";

export interface CreateMenuCategoryRequest {
    readonly name: string;
    readonly imageUrl?: string;
    readonly translations?: Record<Language, CreateMenuCategoryTranslation>;
}

interface CreateMenuCategoryTranslation {
    readonly name: string;
}