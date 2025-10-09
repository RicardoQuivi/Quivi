import { PagedRequest } from "../PagedRequest";

export interface GetAvailabilityMenuItemAssociationsRequest extends PagedRequest {
    readonly availabilityIds?: string[];
    readonly menuItemIds?: string[];
}