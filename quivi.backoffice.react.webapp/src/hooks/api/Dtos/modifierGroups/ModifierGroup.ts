import { Language } from "../Language";

export interface ModifierGroup {
    readonly id: string;
    readonly name: string;
    readonly minSelection: number;
    readonly maxSelection: number;
    readonly items: Record<string, ModifierGroupItem>;
    readonly translations: Record<Language, ModifierGroupTranslation>;
}

export interface ModifierGroupItem {
    readonly price: number;
    readonly sortIndex: number;
}

export interface ModifierGroupTranslation {
    readonly name: string;
}