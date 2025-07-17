import type { PagedRequest } from "../PagedRequest";

export interface GetMenuCategoriesRequest extends PagedRequest {
    readonly channelId: string;
    readonly atDate?: Date;
}