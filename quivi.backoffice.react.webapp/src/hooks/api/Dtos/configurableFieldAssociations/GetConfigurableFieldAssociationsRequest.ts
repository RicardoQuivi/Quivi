import { PagedRequest } from "../PagedRequest";

export interface GetConfigurableFieldAssociationsRequest extends PagedRequest {
    readonly channelProfileIds?: string[];
    readonly configurableFieldIds?: string[];
}