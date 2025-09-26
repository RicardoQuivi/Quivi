export interface MenuItem {
    readonly id: string;
    readonly name: string;
    readonly imageUrl?: string;
    readonly price: number;
    readonly modifierGroups: ModifierGroup[];
    readonly categoryIds: string[];
    readonly hasStock: boolean;
    readonly isDeleted: boolean;
}

export interface ModifierGroup {
    readonly id: string;
    readonly name: string;
    readonly minSelection: number;
    readonly maxSelection: number;
    readonly options: ModifierGroupOption[];
}

export interface ModifierGroupOption {
    readonly id: string;
    readonly menuItemId: string;
    readonly price: number;
}