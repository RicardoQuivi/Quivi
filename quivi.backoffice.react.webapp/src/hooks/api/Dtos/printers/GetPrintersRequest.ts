import { PagedRequest } from "../PagedRequest";

export interface GetPrintersRequest extends PagedRequest {
    readonly printerWorkerId?: string;
    readonly ids?: string[];
}