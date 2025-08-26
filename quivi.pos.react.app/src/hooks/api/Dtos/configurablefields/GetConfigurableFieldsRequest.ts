import { PagedRequest } from "../PagedRequest";

export interface GetConfigurableFieldsRequest extends PagedRequest {
    readonly channelIds?: string[];
    readonly ids?: string[];
    readonly forPosSessions?: boolean;
}