export interface GetMenuItemsRequest {
    readonly ids?: string[];
    readonly itemCategoryId?: string;
    readonly search?: string;
    readonly hasCategory?: boolean;
}