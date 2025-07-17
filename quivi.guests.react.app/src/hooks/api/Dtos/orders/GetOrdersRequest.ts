import type { PagedRequest } from "../PagedRequest";

export interface GetOrdersRequest extends PagedRequest {
    readonly ids?: string[];
    readonly chargeIds?: string[];
    readonly channelIds?: string[];
    readonly sessionId?: string;
}