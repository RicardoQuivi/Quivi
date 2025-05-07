import { PagedRequest } from "../PagedRequest";

export interface GetEmployeesRequest extends PagedRequest {
    readonly ids?: string[];
    readonly includeDeleted?: boolean;
}