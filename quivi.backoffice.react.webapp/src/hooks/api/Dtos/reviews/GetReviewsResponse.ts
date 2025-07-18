import { PagedResponse } from "../PagedResponse";
import { Review } from "./Review";

export interface GetReviewsResponse extends PagedResponse<Review> {
}