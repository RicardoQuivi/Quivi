import { PagedRequest } from "../PagedRequest";

export interface GetChannelsRequest extends PagedRequest {
    readonly ids?: string[];
    readonly search?: string;
    readonly allowsSessionsOnly?: boolean;
}