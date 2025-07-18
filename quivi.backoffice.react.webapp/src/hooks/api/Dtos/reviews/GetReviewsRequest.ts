import { PagedRequest } from "../PagedRequest";

export interface GetReviewsRequest extends PagedRequest {
    readonly ids?: string[];
}