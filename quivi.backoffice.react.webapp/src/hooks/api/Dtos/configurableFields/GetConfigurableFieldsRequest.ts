import { PagedRequest } from "../PagedRequest";

export interface GetConfigurableFieldRequest extends PagedRequest {
    readonly ids?: string[];
}