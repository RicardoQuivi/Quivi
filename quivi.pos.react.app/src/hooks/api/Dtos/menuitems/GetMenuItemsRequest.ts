import { PagedRequest } from "../PagedRequest";

export interface GetMenuItemsRequest extends PagedRequest {
    readonly ids?: string[];
    readonly menuCategoryId?: string;
    readonly search?: string;
    readonly includeDeleted?: boolean;
}