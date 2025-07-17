import type { PagedRequest } from "../PagedRequest";

export interface GetMenuItemsRequest extends PagedRequest {
    readonly channelId: string;
    readonly menuItemCategoryId?: string;
    readonly ids?: string[];
    readonly ignoreCalendarAvailability?: boolean;
    readonly atDate?: Date;
}