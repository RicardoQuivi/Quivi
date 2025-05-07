import { PagedRequest } from "../PagedRequest";

export interface GetLocalsRequest extends PagedRequest{
    readonly ids?: string[];
}