import { Language } from "../Language";

export interface PatchMenuCategoryRequest {
    readonly id: string;
    readonly name?: string;
    readonly imageUrl?: string | null;
    readonly translations?: Record<Language, PatchMenuCategoryTranslation | undefined>;
    readonly menuCategoryIds?: string[];
}

export interface PatchMenuCategoryTranslation {
    readonly name?: string;
}