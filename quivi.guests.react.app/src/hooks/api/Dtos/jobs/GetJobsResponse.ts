import type { DataResponse } from "../DataResponse";
import type { Job } from "./Job";

export interface GetJobsResponse extends DataResponse<Job[]> {
    
}