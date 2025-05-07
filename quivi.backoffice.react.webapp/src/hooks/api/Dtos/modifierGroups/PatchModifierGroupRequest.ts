import { Language } from "../Language";

export interface PatchModifierGroupRequest {
    readonly id: string;
    readonly name?: string;
    readonly minSelection?: number;
    readonly maxSelection?: number;
    readonly items?: Record<string, PatchModifierItem | undefined>;
    readonly translations?: Record<Language, PatchModifierGroupTranslation> | undefined;
}

export interface PatchModifierItem {
    readonly price?: number;
    readonly sortIndex?: number;
}

export interface PatchModifierGroupTranslation {
    readonly name?: string;
}