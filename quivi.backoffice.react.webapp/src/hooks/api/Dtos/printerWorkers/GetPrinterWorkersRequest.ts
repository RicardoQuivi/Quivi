import { PagedRequest } from "../PagedRequest";

export interface GetPrinterWorkersRequest extends PagedRequest {
    readonly ids?: string[];
}