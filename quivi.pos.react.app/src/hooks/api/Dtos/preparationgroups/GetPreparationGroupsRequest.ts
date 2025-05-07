import { PagedRequest } from "../PagedRequest";

export interface GetPreparationGroupsRequest extends PagedRequest {
    readonly sessionIds?: string[];
    readonly locationId?: string;
    readonly isCommited?: boolean;
}