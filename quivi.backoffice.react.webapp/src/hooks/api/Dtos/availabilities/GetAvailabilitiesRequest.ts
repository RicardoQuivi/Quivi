import { PagedRequest } from "../PagedRequest";

export interface GetAvailabilitiesRequest extends PagedRequest {
    readonly ids?: string[];
}