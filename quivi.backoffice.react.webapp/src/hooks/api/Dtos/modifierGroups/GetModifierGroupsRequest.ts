import { PagedRequest } from "../PagedRequest";

export interface GetModifierGroupsRequest extends PagedRequest {
    readonly ids?: string[];
    readonly menuItemIds?: string[];
}