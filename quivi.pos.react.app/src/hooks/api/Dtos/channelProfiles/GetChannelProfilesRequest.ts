import { PagedRequest } from "../PagedRequest";

export interface GetChannelProfilesRequest extends PagedRequest {
    readonly ids?: string[];
    readonly allowsSessionsOnly?: boolean;
}