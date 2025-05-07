import { PagedRequest } from "../PagedRequest";

export interface GetMerchantsRequest extends PagedRequest {
    readonly search?: string;
    readonly parentId?: string;
    readonly id?: string;
}