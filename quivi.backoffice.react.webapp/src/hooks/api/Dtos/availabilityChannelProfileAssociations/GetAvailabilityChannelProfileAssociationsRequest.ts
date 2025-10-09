import { PagedRequest } from "../PagedRequest";

export interface GetAvailabilityChannelProfileAssociationsRequest extends PagedRequest {
    readonly availabilityIds?: string[];
    readonly channelProfileIds?: string[];
}