import { PagedRequest } from "../PagedRequest";

export interface GetCustomChargeMethodsRequest extends PagedRequest {
    readonly ids?: string[];
}