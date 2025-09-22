import { PagedRequest } from "../PagedRequest";
import { SortableRequest } from "../SortableRequest";
import { OrderState } from "./OrderState";

export interface GetOrdersRequest extends PagedRequest, SortableRequest {
    readonly ids?: string[];
    readonly states?: OrderState[];
    readonly channelIds?: string[];
    readonly sessionIds?: string[];
}