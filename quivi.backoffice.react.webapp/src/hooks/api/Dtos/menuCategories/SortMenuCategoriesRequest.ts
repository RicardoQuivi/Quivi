export interface SortMenuCategoriesRequest {
    readonly items: SortedMenuCategoryRequest[];
}

export interface SortedMenuCategoryRequest {
    readonly id: string;
    readonly sortIndex: number;
}