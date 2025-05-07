import { PagedRequest } from "../PagedRequest";

export interface GetPosIntegrationsRequest extends PagedRequest {
    readonly ids?: string[];
}