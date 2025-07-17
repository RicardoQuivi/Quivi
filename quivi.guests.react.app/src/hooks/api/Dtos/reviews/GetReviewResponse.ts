import type { DataResponse } from "../DataResponse";
import type { Review } from "./Review";

export interface GetReviewResponse extends DataResponse<Review | undefined> {
    
}