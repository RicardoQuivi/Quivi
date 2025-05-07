import { PagedRequest } from "../PagedRequest";

export interface GetChannelsRequest extends PagedRequest {
    readonly ids?: string[];
    readonly allowsSessionsOnly?: boolean;
    readonly includeDeleted?: boolean;
    readonly search?: string;

    readonly sessionIds?: string[];
    readonly allowsPrePaidOrderingOnly?: boolean;
    readonly allowsPostPaidOrderingOnly?: boolean;
    readonly includePageRanges?: boolean;
    readonly channelProfileId?: string;
    readonly hasOpenSession?: boolean;
}