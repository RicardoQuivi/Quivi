import { PagedRequest } from "../PagedRequest";

export interface GetMenuItemsRequest extends PagedRequest {
    readonly ids?: string[];
    readonly itemCategoryId?: string;
    readonly search?: string;
    readonly hasCategory?: boolean;
}