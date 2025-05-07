import { Language } from "../Language";

export interface CreateModifierGroupRequest {
    readonly name: string;
    readonly minSelection: number;
    readonly maxSelection: number;
    readonly items: Record<string, CreateModifierItem>;
    readonly translations?: Record<Language, CreateModifierGroupTransaltion>;
}

interface CreateModifierItem {
    readonly price: number;
    readonly sortIndex: number;
}

interface CreateModifierGroupTransaltion {
    readonly name: string;
}