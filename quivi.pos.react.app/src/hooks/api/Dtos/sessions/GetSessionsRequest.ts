import { PagedRequest } from "../PagedRequest";

export interface GetSessionsRequest extends PagedRequest {
    readonly ids?: string[];
    readonly channelIds?: string[];
    readonly isOpen?: boolean;
    readonly includeDeleted?: boolean;
}