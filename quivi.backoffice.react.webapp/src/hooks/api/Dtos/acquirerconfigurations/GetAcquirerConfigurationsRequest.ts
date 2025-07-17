import { PagedRequest } from "../PagedRequest";

export interface GetAcquirerConfigurationsRequest extends PagedRequest {
    readonly ids?: string[];
}