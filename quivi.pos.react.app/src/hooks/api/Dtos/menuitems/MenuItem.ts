export interface MenuItem {
    readonly id: string;
    readonly name: string;
    readonly imageUrl?: string;
    readonly price: number;
    readonly modifierGroups: ModifierGroup[];
}

export interface ModifierGroup {

}