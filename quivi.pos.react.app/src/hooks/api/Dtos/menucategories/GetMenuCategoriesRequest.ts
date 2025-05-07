import { PagedRequest } from "../PagedRequest";

export interface GetMenuCategoriesRequest extends PagedRequest {
    readonly ids?: string[];
    readonly hasItems?: boolean;
}